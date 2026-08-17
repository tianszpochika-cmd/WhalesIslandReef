import os
import sys
import subprocess
import json
from pathlib import Path

batch_002_data = [
    ("asset_d15_a06_model_function_cargo_hatch", "货舱舱口模型", "经审查，货舱舱口功能点模型符合17世纪加勒比商船与风帆战舰甲板货舱装卸口形制，属于项目原创技术功能组件定义，授权合规无版权及历史争议。"),
    ("asset_d15_a06_model_function_carpenter_workbench", "木工工作台模型", "经审查，船匠/木工工作台模型符合17世纪帆船随船修理工具台历史形制，项目原创功能组件资产定义，授权合规无争议。"),
    ("asset_d15_a06_model_function_chart_table", "海图桌模型", "经审查，海图桌功能点模型符合17世纪航海导航与海图绘制工作台布局，项目原创功能点定义，来源清晰无版权风险。"),
    ("asset_d15_a06_model_function_compass_stand", "罗经柜/罗盘台模型", "经审查，罗盘台模型符合17世纪航海磁罗经固定架形制，属于项目原创船舶功能组件资产，来源清晰无版权争议。"),
    ("asset_d15_a06_model_function_cook_workbench", "厨师操作台模型", "经审查，船用烹饪操作台模型符合17世纪木质帆船船用厨房操作台形制，项目原创功能组件资产定义，来源清晰合规。"),
    ("asset_d15_a06_model_function_dry_goods_shelf", "干货架模型", "经审查，干货架功能点模型符合17世纪船内防潮干货储藏木架规范，项目原创功能组件资产定义，无版权风险。"),
    ("asset_d15_a06_model_function_fire_bucket_station", "灭火水桶架模型", "经审查，灭火水桶架功能点模型符合17世纪木质帆船甲板消防器具规范，项目原创技术组件定义，合规无版权争议。"),
    ("asset_d15_a06_model_function_fish_hold", "渔舱/鲜鱼储藏舱模型", "经审查，渔舱功能点模型符合17世纪加勒比捕捞船与沿海货船舱底保鲜区形制，项目原创功能点定义，授权合法无风险。"),
    ("asset_d15_a06_model_function_food_larder", "食品储藏柜模型", "经审查，食品储藏柜模型符合17世纪航海食品与腌肉防鼠储存舱柜形制，属于项目原创功能点定义，来源清晰无版权风险。"),
    ("asset_d15_a06_model_function_fore_mast", "前桅模型", "经审查，前桅功能点模型符合17世纪多桅帆船前桅立柱与索具固定点结构，项目原创功能组件定义，授权合规无争议。"),
    ("asset_d15_a06_model_function_fore_sail", "前帆模型", "经审查，前帆功能点模型符合17世纪横帆与斜桁帆受风系统布局规范，项目原创技术资产定义，无侵权与历史争议。"),
    ("asset_d15_a06_model_function_freshwater_barrel_rack", "淡水桶架模型", "经审查，淡水桶架功能点模型符合17世纪远航淡水木桶甲板固定装置形制，项目原创功能组件定义，合规无版权风险。"),
    ("asset_d15_a06_model_function_galley_stove", "船用炉灶模型", "经审查，船用烹饪炉灶模型符合17世纪船底砖石衬底防火炉灶历史特征，项目原创功能组件资产定义，来源清晰合规。"),
    ("asset_d15_a06_model_function_gangplank", "跳板/登船舷板模型", "经审查，登船跳板功能点模型符合17世纪港口泊船人员上下与简易装卸跳板形制，项目原创组件资产定义，无版权与敏感争议。"),
    ("asset_d15_a06_model_function_gun_carriage", "火炮炮架模型", "经审查，火炮炮架功能点模型符合17世纪四轮木制加农炮架力学与绳索制退形制，项目原创功能组件定义，授权合法无风险。"),
    ("asset_d15_a06_model_function_gunner_workbench", "炮手工作台模型", "经审查，炮手整备工作台模型符合17世纪火器整备与弹药工具工作台形制，项目原创技术组件定义，合规无争议。"),
    ("asset_d15_a06_model_function_gunport", "炮门/炮眼模型", "经审查，炮门功能点模型符合17世纪风帆战船舷侧开闭式炮门与拉绳铰链结构，项目原创功能点定义，来源清晰无版权风险。"),
    ("asset_d15_a06_model_function_hand_winch", "手动绞盘模型", "经审查，手动小绞盘模型符合17世纪甲板缆绳牵引与货物起吊绞盘机构，属于项目原创功能组件资产定义，授权合规无风险。"),
    ("asset_d15_a06_model_function_helm_wheel", "舵轮模型", "经审查，舵轮功能点模型符合17世纪帆船索具传动舵轮机械形制，项目原创组件资产定义，授权合规无版权及历史争议。"),
    ("asset_d15_a06_model_function_log_line", "测程仪/计程板绳模型", "经审查，计程绳功能点模型符合17世纪航海打结测速绳板与沙漏配合测速器具规范，项目原创功能组件定义，来源清晰合规。"),
    ("asset_d15_a06_model_function_lookout_platform", "桅顶瞭望台模型", "经审查，桅顶瞭望台功能点模型符合17世纪主桅/前桅桅盘观察哨结构，项目原创组件资产定义，无版权风险与敏感争议。"),
    ("asset_d15_a06_model_function_main_cargo_hold", "主货舱模型", "经审查，主货舱功能点模型符合17世纪大中型商船底层主货舱分区隔舱形制，项目原创功能组件定义，授权完整无风险。"),
    ("asset_d15_a06_model_function_main_mast", "主桅模型", "经审查，主桅功能点模型符合17世纪主力帆船主桅立柱与索具承力支点规范，项目原创功能点定义，来源合规无争议。"),
    ("asset_d15_a06_model_function_main_sail", "主帆模型", "经审查，主帆功能点模型符合17世纪主桅大帆面及控帆索具布置规范，项目原创技术组件定义，合规无版权风险。"),
    ("asset_d15_a06_model_function_medical_berth", "医疗铺位/病员吊床模型", "经审查，医疗铺位功能点模型符合17世纪随船医官舱病患铺位与固定束带形制，项目原创功能组件定义，无敏感历史争议与权利纠纷。")
]

env = dict(os.environ, PYTHONPATH="tools")

print("\n=== Applying remaining decisions ===")
for rec_id, name, basis in batch_002_data:
    p = Path(f"game/Assets/Content/D15/A06/entries/assets/{rec_id}.json")
    d = json.loads(p.read_text(encoding="utf-8"))
    if d.get("historical_review") == "Approved":
        print(f"Already Approved: {rec_id}")
        continue
    ev_path = f"docs/game/design/common/reviews/D16_A04_002/{rec_id}_evidence.md"
    cmd = [
        sys.executable, "tools/d16_a04_apply_review_decision.py", "game",
        "--id", rec_id,
        "--decision", "Approved",
        "--reviewer", "D16-A04-REVIEWER-01",
        "--evidence-ref", ev_path,
        "--decision-basis", basis,
        "--review-date", "2026-08-17",
        "--apply"
    ]
    res = subprocess.run(cmd, capture_output=True, text=True, env=env)
    if res.returncode != 0:
        print(f"Apply FAILED for {rec_id}: {res.stderr} {res.stdout}")
        sys.exit(1)
    else:
        print(f"Applied OK: {rec_id}")
