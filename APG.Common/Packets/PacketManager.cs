using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APG.Common.Packets.Types;
using Newtonsoft.Json;

namespace APG.Common.Packets
{
    public class PacketManager
    {
        //max is 2 130 702 268
        public const int MAX_BUFFER_SIZE = 100000; //Change this to lower....

        private Dictionary<Guid, List<Packet>> splitPackets = new Dictionary<Guid, List<Packet>>();

        public Packet UnpackPacket(byte[] buffer, int bytes)
        {
            var packet = Packet.Unpack(buffer, bytes);

            if (packet.IsSplitPacket())
            {
                if(!splitPackets.ContainsKey(packet.ID))
                    splitPackets.Add(packet.ID, new List<Packet>());

                var splitPacketsList = splitPackets[packet.ID];
                
                splitPacketsList.Add(packet);

                if (splitPacketsList.Count == packet.Size) //Check if all required split packets are here
                {
                    //Rearrange them by their correct order (split packet number) and then merge them in one single packet
                    Type completeDataType = packet.GetData<SplitPacket>().DataType;
                    var completeData = Array.Empty<byte>();
                    foreach (var splitPacket in splitPacketsList.OrderBy(sp => sp.PartNumber))
                    {
                        var splitData = splitPacket.GetData<SplitPacket>();
                        completeData = completeData.Concat(splitData.Chunk).ToArray();
                    }

                    var json = Encoding.ASCII.GetString(completeData);

                    return new Packet(packet.ID, JsonConvert.DeserializeObject(json, completeDataType));
                }
            }
            else
            {
                return packet;
            }

            return null;
        }

        public List<byte[]> PackPacket(Packet packetToSend)
        {
            List<byte[]> packedPackets = new List<byte[]>();

            var packed = packetToSend.Pack();

            //The split packet functionality is not working as the calculation to split the data and metadata are not consistent, making the packets always bigger the allowed size,
            //This is a bug that needs to be fixed eventually
            if (packed.Length >= MAX_BUFFER_SIZE) //If the packet to send is bigger than the alloweded send size
            {

                //This Logic is not working as intented
                var completeData = packetToSend.GetDataBytes();
                var minimumForSplitPacket = (packed.Length - (packed.Length - completeData.Length)) * 1.50f; // Allocate 50%(?) of allowed size for metadata
                int maxDataBytesPerPacket = (int) MathF.Ceiling((MAX_BUFFER_SIZE - minimumForSplitPacket) * 0.5f); //Allocate 50% of allowed size for data
                var requiredPackets = MathF.Ceiling(completeData.Length / (float)maxDataBytesPerPacket);

                var bytesLeft = completeData.Length;
                for (int i = 0; i < requiredPackets; i++)
                {
                    var lengthToCopy = bytesLeft > maxDataBytesPerPacket ? maxDataBytesPerPacket : bytesLeft;
                    bytesLeft -= maxDataBytesPerPacket;

                    var chunk = new byte[lengthToCopy];
                    Array.Copy(
                        completeData,i * maxDataBytesPerPacket,
                        chunk, 0,
                        lengthToCopy);
                    
                    var splitPacket = 
                        new Packet(
                            packetToSend.ID, (
                                int)requiredPackets, 
                            (i + 1), 
                            new SplitPacket(packetToSend.DataType, chunk)
                            );
                    
                    var buffer = new byte[MAX_BUFFER_SIZE];
                    var splitedPackedPacket = splitPacket.Pack();

                    if (splitedPackedPacket.Length > MAX_BUFFER_SIZE)
                        throw new Exception($"TOO BIG by: {splitedPackedPacket.Length - MAX_BUFFER_SIZE}");

                    splitedPackedPacket.CopyTo(buffer,0);
                    packedPackets.Add(buffer);
                }

            }
            else
            {
                var buffer = new byte[MAX_BUFFER_SIZE];
                packed.CopyTo(buffer,0);
                packedPackets.Add(buffer);
            }

            return packedPackets;
        }

    }
}
