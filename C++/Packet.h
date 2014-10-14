#pragma once
#include <stdint.h>
#include <stdbool.h>
#include <float.h>
#include <vector>
namespace Insanedev
{
	namespace Networking
	{

		class Packet
		{
		public:

			enum ParamTypes
			{
				FLOAT,
				DOUBLE,
				INT16,
				UINT16,
				INT32,
				UINT32,
				INT64,
				UINT64,
				BOOL,
				BYTE_PACKET,
				UTF8_STRING,
				COMPRESSED_BYTE_PACKET,
			};

			Packet(uint16_t type);
			~Packet();

			void AddFloat(const float_t value);
			void AddDouble(const double_t value);
			void AddInt16(const int16_t value);
			void AddUint16(const uint16_t value);
			void AddInt32(const int32_t value);
			void AddUint32(const uint32_t value);
			void AddInt64(const int64_t value);
			void AddUint64(const uint64_t value);

			const uint32_t ToByteArray(uint8_t ** dataPointer);
			std::vector<void *> GetObjects() const;
			uint16_t GetType() const;

			static Packet * FromByteArray(const uint8_t * data);
			static const uint8_t PacketStartBytes[4];


		protected:
			uint8_t * _Data = nullptr;
			uint32_t _DataArraySize = 0;
			uint32_t _DataPos;
			std::vector<void *> _PackedObjects;
			uint16_t _ParamCount;
			uint8_t * _ReturnByteArray = nullptr;
			uint32_t _ReturnByteArraySize = 0;

		private:
			Packet();
			uint16_t _Type;
			void DestroyReturnByteArray();
			void ExpandDataArray();
			template <typename T>
			T * GetDataFromArray(int offset);
			template <typename T>
			void AddToDataArray(ParamTypes type, int32_t dataAmmount, T const*  dataPosition);
			void UpdateObjects();
		};

	}
}