using NetWorkModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_player_Whack_a_Mole
{
    public class PacketHandler
    {

        public static void C_LoginHandler(IPeer peer, C_Login packet)
        {
            // 기존 로그인 핸들러 로직 유지
            GameUser session = peer as GameUser;
            session.SendLoginOk();

        }
        public static void C_EnterGameHandler(IPeer peer, C_EnterGame packet)
        {
            // ID 발급 로직 (필요하다면 유지, 아니면 DB ID 사용)
            // session.UserID = ...; 
            GameUser session = peer as GameUser;
            // 로그인 성공 패킷 전송
            //S_LoginOk sendPacket = new S_LoginOk();
            //session.send(sendPacket.Write());

            // 매칭 요청 (기존 로직 호출)
            // Handler는 '연결'만 해주고, 실제 무거운 일은 다른 애한테 시킵니다.
            Program.game_main.matching_req(session);
        }

        // 2. 로딩 완료 (LOADING_COMPLETED 대체)
        public static void C_LoadingCompleteHandler(IPeer peer, C_LoadingComplete packet)
        {
            GameUser session = peer as GameUser;
            if (session.battle_room != null)
            {
                // 방으로 로직 위임
                session.battle_room.loading_complete(session.MyPlayer);
            }
        }

        // 3. 두더지 타격 (HIT_MOLE_REQ 대체)
        public static void C_HitMoleHandler(IPeer peer, C_HitMole packet)
        {
            // [장점] 더 이상 pop_int32() 순서를 신경 쓸 필요가 없습니다!
            // GenPackets가 이미 다 파싱해서 'packet' 변수에 담아왔기 때문입니다.
            GameUser session = peer as GameUser;
            int hitIndex = packet.moleIndex; // 변수명으로 바로 접근!
            
            int userId = packet.UserID;
            if (session.battle_room != null)
            {
                session.battle_room.HandleHit(session, hitIndex, userId);
            }
        }
    }
}
