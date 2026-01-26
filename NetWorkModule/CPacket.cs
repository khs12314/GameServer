using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetWorkModule
{
    public class CPacket
    {
        public IPeer owner { get; private set; }
        public byte[] buffer { get; private set; }
        public int position { get; private set; }
        public Int16 protocol_id { get;  set; }
        public bool is_using = true;
        private int _refCount = 0;
        public static CPacket create(Int16 protocol_id)
        {
            CPacket packet = CPacketBufferManger.pop();
            packet.is_using = true;
            packet._refCount = 1;
            packet.set_protocol(protocol_id);
            
            return packet;
        }
        public static CPacket create(byte[] buffer, IPeer owner)
        {
            CPacket packet = CPacketBufferManger.pop();
            packet._refCount = 1;
            packet.is_using = true;
            packet.owner = owner;
            packet.overwrite(buffer, buffer.Length);
            packet.protocol_id = BitConverter.ToInt16(buffer, 2);
            
            return packet;
        }
        public void Retain()
        {
            Interlocked.Increment(ref _refCount);
        }
        public void Release()
        {
            //CPacketBufferManger.get_pool_count();
            if (Interlocked.Decrement(ref _refCount) == 0)
            {
                this.is_using = false;
                this.position = 0;
                CPacketBufferManger.push(this);
            }
        }

        public static void destroy(CPacket packet)
        {
            if (packet.is_using == false)
            {
                return;
            }

            packet.is_using = false;
            
            CPacketBufferManger.push(packet);
        }
        public CPacket(byte[] buffer, IPeer owner)
        {
            //참조로만 보관하여 작업한다.
            //복사가 필요하면 별도로 구현
            this.buffer = buffer;
            //헤더는 읽을 필요 없으니 그 이후부터 시작한다.
            this.position = Define.HEADER_SIZE;
            this.owner = owner;
            this.protocol_id = BitConverter.ToInt16(buffer, 2);
        }
        public CPacket()
        {
            this.buffer = new byte[1024];
        }
        public Int16 pop_protocol_id()
        {
            
            return pop_int16();
        }
        public void copy_to(CPacket target)
        {
            target.set_protocol(this.protocol_id);
            target.overwrite(this.buffer, this.position);

        }
        public void overwrite(byte[] source, int length)
        {
            // 1. 버퍼 크기 확보 (안전장치)
            // 내 버퍼가 없거나 작으면 새로 만듬. (풀링 중이라면 이미 충분히 클 것임)
            if (this.buffer == null || this.buffer.Length < length)
            {
                this.buffer = new byte[length > 1024 ? length : 1024];
            }

            // 2. [Deep Copy] 데이터 복사
            // "외부 소스(source)의 0번지부터 length만큼" -> "내 안방(buffer)의 0번지"로 복사
            Array.Copy(source, 0, this.buffer, 0, length);

            // 3. [Cursor Reset] 읽기 준비
            // 데이터를 새로 썼으니, 읽는 위치는 다시 맨 앞(헤더 뒤)으로 돌아가야 함.
            // 이걸 안 하면 position이 엉뚱한 곳에 가 있어서 에러 남.
            this.position = Define.HEADER_SIZE;
        }
        public void reset_position()
        {
            this.position = 0;
            
        }

        // 혹은 원하는 위치로 이동시키는 메서드
        public void move_position(int pos)
        {

            this.position = pos;
           
        }
        public byte pop_byte()
        {
            byte data = this.buffer[this.position];
            this.position += sizeof(byte);
            return data;
        }
        public bool pop_bool()
        {
            byte data = this.buffer[this.position];
            this.position += sizeof(byte);
            return (data != 0);
        }
        public Int16 pop_int16()
        {
            
            ReadOnlySpan<byte> SpanSpace = new ReadOnlySpan<byte>(this.buffer, this.position, sizeof(Int16));
            Int16 data = BitConverter.ToInt16(SpanSpace);
            this.position += sizeof(Int16);
            
            return data;
        }
        public Int32 pop_int32()
        {
            ReadOnlySpan<byte> SpanSpace = new ReadOnlySpan<byte>(this.buffer, this.position, sizeof(Int32));
            Int32 data = BitConverter.ToInt32(SpanSpace);
            this.position += sizeof(Int32);
            return data;
        }
       
        public string pop_string()
        {
            //문자열 최대 2바이트까지 0~32767
            Int16 len = pop_int16();
            ReadOnlySpan<byte> SpanSpace = new ReadOnlySpan<byte>(this.buffer, this.position, len);
            string data = Encoding.UTF8.GetString(SpanSpace);
            this.position += len;
            return data;
        }
        public long pop_long()
        {
            ReadOnlySpan<byte> SpanSpace = new ReadOnlySpan<byte>(this.buffer, this.position, sizeof(long));
            long data = BitConverter.ToInt64(SpanSpace);
            this.position += sizeof(long);
            return data;
        }
        public float pop_float()
        {
            ReadOnlySpan<byte> SpanSpace = new ReadOnlySpan<byte>(this.buffer, this.position, sizeof(float));
            float data = BitConverter.ToSingle(SpanSpace);
            this.position += sizeof(float);
            return data;
        }
        public double pop_double()
        {
            ReadOnlySpan<byte> SpanSpace = new ReadOnlySpan<byte>(this.buffer, this.position, sizeof(double));
            double data = BitConverter.ToDouble(SpanSpace);
            this.position += sizeof(double);
            return data;
        }
        public Int64 pop_int64()
        {
            ReadOnlySpan<byte> SpanSpace = new ReadOnlySpan<byte>(this.buffer, this.position, sizeof(Int64));
            Int64 data = BitConverter.ToInt64(SpanSpace);
            this.position += sizeof(Int64);
            return data;
        }
        public void set_protocol(Int16 protocol_id)
        {
            this.protocol_id = protocol_id;
            
            //헤더는 나중에 넣을것이므로 데이터부터 넣을  수있도록 위치 점프
            this.position = Define.HEADER_SIZE;
            push_int16(protocol_id);
            
            
        }
        public void record_size()
        {
            Int16 body_size = (Int16)(this.position - Define.HEADER_SIZE);
            //Console.WriteLine($"바디 사이즈{body_size}, 포지션원본 {this.position}");
            Span<byte> SpanSpace = new Span<byte>(this.buffer, 0, sizeof(Int16));   
            BitConverter.TryWriteBytes(SpanSpace, body_size);
            //Console.WriteLine($"바디 사이즈{body_size}, 포지션바디사이즈 후 {this.position}");
        }
       
        public void push_int16(Int16 data)
        {
            Span<byte> SpanSpace = new Span<byte>(this.buffer, this.position, sizeof(Int16));
            bool success = BitConverter.TryWriteBytes(SpanSpace, data); 

            if (success)
            {
                this.position += sizeof(Int16);
                return;
            }
            else
            {
                
            }
        }
        public void push(bool data)
        {
            this.buffer[this.position] = (byte)(data ? 1 : 0);
            this.position += sizeof(byte);

        }
        public void push(byte data)
        {
            this.buffer[this.position] = data;
            this.position += sizeof(byte);
        }

        public void push(Int16 data)
        {
            Span<byte> SpanSpace = new Span<byte>(this.buffer, this.position, sizeof(Int16));  
            bool success = BitConverter.TryWriteBytes(SpanSpace, data); 

            if (success)
            {
                this.position += sizeof(Int16);
                return;
            }
        }

        public void push(Int32 data)
        {
            Span<byte> SpanSpace = new Span<byte>(this.buffer, this.position, sizeof(Int32));
            bool success = BitConverter.TryWriteBytes(SpanSpace, data);
            
            if (success)
            {
                this.position += sizeof(Int32);
                return;
            }
        }
        public void push(Int64 data)
        {
            Span<byte> SpanSpace = new Span<byte>(this.buffer, this.position, sizeof(Int64));
            bool success = BitConverter.TryWriteBytes(SpanSpace, data);
            if (success)
            {
                this.position += sizeof(Int64);
                return;
            }
        }
        public void push(float data)
        {
            Span<byte> SpanSpace = new Span<byte>(this.buffer, this.position, sizeof(float));
            bool success = BitConverter.TryWriteBytes(SpanSpace, data);
            if (success)
            {
                this.position += sizeof(float);
                return;
            }
        }
        public void push(double data)
        {
            Span<byte> SpanSpace = new Span<byte>(this.buffer, this.position, sizeof(double));
            bool success = BitConverter.TryWriteBytes(SpanSpace, data);
            if (success)
            {
                this.position += sizeof(double);
                return;
            }
        }
        

        public void SetOwner(IPeer peer)
        {
            this.owner = peer;
        }
        public void push(string data)
        {
            int byteCount = Encoding.UTF8.GetByteCount(data);

            push((Int16)byteCount);

            Span<byte> SpanSpace = new Span<byte>(this.buffer, this.position, byteCount);

            Encoding.UTF8.GetBytes(data, SpanSpace);

            this.position += byteCount;

        }
    }
}
