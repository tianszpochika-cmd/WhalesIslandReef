// ============================================================
// ShipTypeDef —— 船型定义（纯数据，对应 DATA_MODELS §5）
//
// 归属：5 船只系统（当前由 Save 域负责建表与种子落库，P2 船只系统接手运行时消费）
// 史实锚定（1675 加勒比）：
//   - small_fishing 小渔船：沿海小渔船造价 30-50 英镑（≈300 银币），吃水 <1m 可进浅湾
//   - sloop 单桅纵帆船：加勒比贸易/私掠主力（巴巴多斯造 200-400 英镑 ≈ 3000 银币），
//     吃水 ~1.5-2m，航速快、转向灵，可载 8 门轻炮
// ============================================================

namespace Whalebone.Systems.Save
{
    /// <summary>
    /// 船型定义（内存形态 = ship_type_defs 表一行）。
    /// </summary>
    public class ShipTypeDef
    {
        public string Id;
        public string Name;
        public float Speed;       // 航速（节，游戏内归一化）
        public float Turn;        // 转向（越高越灵）
        public int CargoMax;      // 载重上限
        public int CrewMax;       // 船员上限
        public int CannonMax;     // 炮位上限
        public int HullMax;       // 耐久上限
        public float Draft;       // 吃水（米，近岸/浅滩判定）
        public int Price;         // 造价（银币）

        public ShipTypeDef() { }

        public ShipTypeDef(string id, string name, float speed, float turn,
                           int cargoMax, int crewMax, int cannonMax, int hullMax,
                           float draft, int price)
        {
            Id = id;
            Name = name;
            Speed = speed;
            Turn = turn;
            CargoMax = cargoMax;
            CrewMax = crewMax;
            CannonMax = cannonMax;
            HullMax = hullMax;
            Draft = draft;
            Price = price;
        }
    }
}
