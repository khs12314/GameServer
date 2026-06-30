using GridTest.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTest.Manager
{
    public static class ChannelManager
    {
        private static Dictionary<int, Channel> _channels = new Dictionary<int, Channel>();
        public static void InitChannels(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _channels[i] = new Channel(i);                
            }
        }

        // 유저가 채널 이동을 요청할 때 안전하게 채널을 찾아주는 함수
        public static Channel GetChannel(int channelId)
        {
            if (_channels.TryGetValue(channelId, out Channel channel))
                return channel;
            return null;
        }

        // 글로벌 타이머가 "순회(Read)"만 할 수 있도록 값들만 넘겨줌
        public static IEnumerable<Channel> GetAllChannels()
        {
            return _channels.Values;
        }
    }
}
