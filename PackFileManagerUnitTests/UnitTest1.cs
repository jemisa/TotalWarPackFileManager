using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using CommonUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PackFileManagerUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var dataFolder = @"C:\Program Files (x86)\Steam\steamapps\common\Total War WARHAMMER II\data";
            var dataFiles = Directory.GetFiles(dataFolder, "*.pack");
            var codec = new PackFileCodec();
            var res = codec.Open(dataFolder + "\\" + "warmachines.pack");
            var fileList = res.Files;

            var filteredList = fileList.Where(x => x.FileExtention == "rigid_model_v2").ToList();

            var outputDirectory = @"c:\temp\datafiles";


            var fileExtractor = new FileExtractor( outputDirectory);
            fileExtractor.ExtractFiles(filteredList, true);

            /* using (StreamWriter writer = new StreamWriter(fileListDialog.FileName))
             {
                 foreach (PackedFile file in currentPackFile.Files)
                 {
                     writer.WriteLine(file.FullPath);
                 }
             }*/
        }



        char[] ToCharArray(byte[] array, int startIndex, int count)
        {
            var output = new char[count];
            for(int i = 0; i < count; i++)
                output[i] = (char)array[startIndex + i];
            return output;
        }


        [TestMethod]
        public void ReadData()
        {
            //
            //var modelDir = @"C:\temp\datafiles\dwf_gyrocopter_bomb_01.rigid_model_v2";
            var modelDir = @"C:\temp\datafiles\dwf_gyrocopter_pilot_head_03.rigid_model_v2";
            var byteArray = File.ReadAllBytes(modelDir);

            var signature = ToCharArray(byteArray, 0, 4);
            var packed_file_header_model_type = BitConverter.ToUInt32(byteArray, 4);
            var packed_file_header_lods_count = BitConverter.ToUInt32(byteArray, 8);
            var packed_file_data_base_skeleton = ToCharArray(byteArray, 12, 132);
            var test = ToCharArray(byteArray, 144, 2);

            var offset = 28;
            var lodStart = 140;
            var currentLodOffset = lodStart;// + offset;
            for (int i = 0; i < packed_file_header_lods_count; i++)
            { 
                var groupCount = BitConverter.ToUInt32(byteArray, currentLodOffset);
                var vertices_data_length = BitConverter.ToUInt32(byteArray, currentLodOffset + 4);
                var indices_data_length = BitConverter.ToUInt32(byteArray, currentLodOffset + 8);
                var start_offset = BitConverter.ToUInt32(byteArray, currentLodOffset + 12);
                var lod_zoom_factor = BitConverter.ToSingle(byteArray, currentLodOffset + 16);

                var mysterious_data_1 = BitConverter.ToUInt32(byteArray, currentLodOffset + 20);
                var mysterious_data_2 = BitConverter.ToUInt32(byteArray, currentLodOffset + 24);


                // 
                var lodOffset = (int)start_offset;
                var materialId = BitConverter.ToUInt32(byteArray, lodOffset);
                var unknown_0 = BitConverter.ToUInt32(byteArray, lodOffset + 4);
                var unknown_1 = BitConverter.ToUInt32(byteArray, lodOffset + 8);
                var vertexCount = BitConverter.ToUInt32(byteArray, lodOffset + 12);
				var unknown_3 = BitConverter.ToUInt32(byteArray, lodOffset + 16);
				var indexCount = BitConverter.ToUInt32(byteArray, lodOffset + 20);


				float GroupMinimumX = BitConverter.ToSingle(byteArray, lodOffset + 24);
				float fGroupMinimumY = BitConverter.ToSingle(byteArray, lodOffset + 28);
				float fGroupMinimumZ = BitConverter.ToSingle(byteArray, lodOffset + 32);
				float fGroupMaximumX = BitConverter.ToSingle(byteArray, lodOffset + 36);
				float fGroupMaximumY = BitConverter.ToSingle(byteArray, lodOffset + 40);
				float fGroupMaximumZ = BitConverter.ToSingle(byteArray, lodOffset + 44);

				var pchShaderName = ToCharArray(byteArray, lodOffset + 48, 12);
				var unknownBlock = ToCharArray(byteArray, lodOffset + 60, 20);

				var wVertexFormat = BitConverter.ToUInt16(byteArray, lodOffset+  80);
				var groupName = ToCharArray(byteArray, lodOffset + 82, 32);

				var textureDirectory = ToCharArray(byteArray, lodOffset + 114, 256);   //112?


				var unknown_4 = ToCharArray(byteArray, lodOffset + 370, 422);
				var uiSupplementarBonesCount = BitConverter.ToUInt32(byteArray, lodOffset + 792);
				var textureCount = BitConverter.ToUInt32(byteArray, lodOffset + 796);

				var unknow_5 = ToCharArray(byteArray, lodOffset + 800, 140);


				var offsetOfterBones = ReadBoneData(byteArray, lodOffset + 940, (int)uiSupplementarBonesCount, out var boneDatas);
				var offsetAfterTextures = ReadTextureData(byteArray, offsetOfterBones, (int)textureCount, out var textureDatas);



				/*
				 
			 switch (m_File.LodData[lod][group]->Header.wVertexFormat)
			{
				case vertex_format::default_format: vertex_size = 32;
					break;
				case vertex_format::weighted_format: vertex_size = 28;
					break;
				case vertex_format::cinematic_format: vertex_size = 32;
			
			}
			 */

			}

			return;
          
        }

		class BoneData
		{
			public char[] Name;
			public char[] Data;
			public int Id;
		}
		class TextureData
		{
			public char[] Name;
			public int Type;
		}

		int ReadBoneData(byte[] byteArray, int boneDataStart, int boneCount, out List<BoneData> boneData)
		{
			boneData = new List<BoneData>();

			var offset = 0;
			for (int i = 0; i < boneCount; i++)
			{
				var name = ToCharArray(byteArray, boneDataStart + offset, 32);
				var unknown = ToCharArray(byteArray, boneDataStart + offset + 32, 48);
				var id = BitConverter.ToUInt32(byteArray, boneDataStart + offset + 80);
				offset += 84;


				boneData.Add(new BoneData()
				{
					Name = name,
					Data = unknown,
					Id = (int)id
				}
				);
			}
			return boneDataStart + offset;
		}

		int ReadTextureData(byte[] byteArray, int textureDataStart, int textureCount, out List<TextureData> textureData)
		{
			textureData = new List<TextureData>();

			var offset = 0;
			for (int i = 0; i < textureCount; i++)
			{
				var type = BitConverter.ToUInt32(byteArray, textureDataStart + offset);
				var name = ToCharArray(byteArray, textureDataStart + offset + 4, 256);
				offset += 260;


				textureData.Add(new TextureData()
				{
					Name = name,
					Type = (int)type
				}
				);
			}
			return textureDataStart + offset;
		}
	}
}

/*
 
     
bool rmv2_file::readLodDataHeaderItems(dword lod, dword group)
{
	
	 
	#define read_item(item) readBuffer(&m_File.LodData[lod][group]->Header.##item, sizeof(m_File.LodData[lod][group]->Header.##item));

	#define item(_item) m_File.LodData[lod][group]->Header.##_item

	   
	//readBuffer(&m_File.LodData[lod][group]->Header.uiMaterialId, sizeof(uint32));

	//uint32 uiMaterialId;		// 4 Bytes[UInt32] - ? // Looks like and ID or a flag, it's always 65 for the moment.
	read_item(uiMaterialId);

	//uint32 uiUnkown;			// No idea. It's a very big number normally.	
	read_item(uiUnkown);

	//uint32 uiUnkown1_1;			// No idea. It's a very big number normally.
	read_item(uiUnkown1_1);	

	//uint32 uiVerticesCount;		// 4 Bytes[UInt32] - VerticesCount
	read_item(uiVerticesCount);

	//(uint32 uiUnkown2_1;			// No idea. It's a very big number normally.
	read_item(uiUnkown2_1);
	
	//uint32 uiIndicesCount;		// 	4 Bytes[UInt32] - IndicesCount
	read_item(uiIndicesCount);

	// offset 24

	//float GroupMinimumX;
	read_item(GroupMinimumX);
	
	//float fGroupMinimumY;
	read_item(fGroupMinimumY);
	
	//float fGroupMinimumZ;
	read_item(fGroupMinimumZ);

	//float fGroupMaximumX;
	read_item(fGroupMaximumX);
	
	//float fGroupMaximumY;
	read_item(fGroupMaximumY);

	//float fGroupMaximumZ;
	read_item(fGroupMaximumZ);

	// offset 48
	   
	//char  pchShaderName[12];
	item(ShaderName) = m_uiBufferOffset; // save offset
	read_item(pchShaderName);		
	item(ShaderName) = item(pchShaderName);  // save text
	
	//  0

	//offset 60


	//char pBlocknown4_20[20];  //offset of the item MAY be determined of the length of the previous string
	read_item(pBlocknown4_20);
	
	//offset 80

	
	read_item(wVertexFormat);
	
	
	//char pchGroupName[32];		// 32 Bytes [0-Padded String] - GroupName
	item(GroupName) = m_uiBufferOffset; // save offset
	read_item(pchGroupName);	
	item(GroupName) = item(pchGroupName);  // save text
	

	
	//char pchTexturesDirectory[256];		// 256 Bytes[0 - Padded String] - TexturesDirectory
	item(TextureDirectory) = m_uiBufferOffset; // save offset
	read_item(pchTextureDirectory);
	item(TextureDirectory) = item(pchTextureDirectory); // save text

	// 256
	
	// jump 56 and read this and then jump back                                                                                                                                                                                                                                               


	//char Unknown3_422[422];//422 Bytes - ? // 422 bytes of perplexity... shader settings? 4 bytes in the middle of this block change if
	read_item(Unknown3_422);

	//I scale the whole model so it's probably not a single block!
	
	//uint32 uiSupplementarBonesCount; //4 Bytes[UInt32] - SupplementarBonesCount
	read_item(uiSupplementarBonesCount);


	//int32 uiTextureCount; //4 Bytes[UInt32] - TexturesCount
	read_item(uiTextureCount);

	//char Unknown140[140];	//140 Bytes - ? // No idea.
	read_item(Unknown140);

	//uint32_t uiUnknow_1_1;
	//read_item(uiUnknow_1_1);


	return true;
};

     
     */













