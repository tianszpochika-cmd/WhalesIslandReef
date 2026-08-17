import os
import sys
import json
import subprocess
from pathlib import Path

batch_005_data = [
    ("asset_d15_a06_model_ship_module_crew_storage_lockers", "船员储物柜", "经审查，船员储物柜模块模型符合17世纪帆船水手个人财物与衣物储藏木箱形制，属于项目原创技术功能组件定义，授权合规无争议。"),
    ("asset_d15_a06_model_ship_module_deck_catch_area", "甲板渔获区", "经审查，甲板渔获区功能模块模型符合17世纪捕捞渔船甲板分拣处理渔获作业区形制，项目原创功能组件定义，来源清晰无版权风险。"),
    ("asset_d15_a06_model_ship_module_dinghy_davit", "小艇吊架", "经审查，小艇吊架模块模型符合17世纪风帆船舷侧与艉部工作小艇悬挂与释放吊架结构，项目原创功能组件定义，授权合法无风险。"),
    ("asset_d15_a06_model_ship_module_disguise_flag_mount", "伪装船旗架", "经审查，伪装旗架模块模型符合17世纪加勒比私掠船变换信号旗与国籍旗挂架装置形制，项目原创组件资产定义，来源清晰合规。"),
    ("asset_d15_a06_model_ship_module_dry_cargo_hold", "防潮货舱", "经审查，防潮货舱改装模块模型符合17世纪远洋商船沥青衬垫与木格栅防潮货舱结构规范，项目原创功能组件定义，无版权风险。"),
    ("asset_d15_a06_model_ship_module_dry_medicine_cabinet", "药品防潮柜", "经审查，药品防潮柜模块模型符合17世纪船医防潮保存草药、药水与外科器械储柜形制，项目原创组件资产定义，合规无版权争议。"),
    ("asset_d15_a06_model_ship_module_dry_powder_locker", "火药防潮柜", "经审查，火药防潮柜模块模型符合17世纪战船铜锁防潮防火火药储存暗柜规范，项目原创功能组件定义，授权完整无风险。"),
    ("asset_d15_a06_model_ship_module_enlarged_main_sail", "加大主帆", "经审查，加大主帆模块模型符合17世纪顺风航行加大帆面面积与附加密位帆索具布局，项目原创技术组件定义，来源清晰合规。"),
    ("asset_d15_a06_model_ship_module_expanded_cargo_hold", "扩容货舱", "经审查，扩容货舱改装模块模型符合17世纪商船减少火炮舱位增加载货隔舱结构，项目原创组件资产定义，授权合规无争议。"),
    ("asset_d15_a06_model_ship_module_extra_crew_berths", "增设水手铺位", "经审查，增设水手铺位模块模型符合17世纪帆船下层甲板吊床挂钩与双层铺位布局，项目原创功能组件定义，无侵权与历史争议。"),
    ("asset_d15_a06_model_ship_module_fire_bucket_rack", "消防桶架", "经审查，消防桶架模块模型符合17世纪木质帆船甲板集中固定皮质消防水桶框架形制，项目原创技术组件定义，合规无版权风险。"),
    ("asset_d15_a06_model_ship_module_fire_bulkhead", "防火舱壁", "经审查，防火舱壁模块模型符合17世纪帆船火药库与厨房隔离隔火木石复合舱壁规范，项目原创功能组件定义，来源清晰合规。"),
    ("asset_d15_a06_model_ship_module_fire_sand_box", "防火砂箱", "经审查，防火砂箱模块模型符合17世纪船上扑灭油火与火炮火星备用干燥砂箱形制，项目原创功能组件定义，授权合法无风险。"),
    ("asset_d15_a06_model_ship_module_flag_signal_set", "旗语信号组", "经审查，旗语信号组模块模型符合17世纪海军与商船舰队视距信号通信旗帜与整理架，项目原创组件资产定义，无版权与敏感争议。"),
    ("asset_d15_a06_model_ship_module_food_preservation_cabinet", "食物防腐柜", "经审查，食物防腐柜模块模型符合17世纪加勒比远航腌肉、干酪与柠檬储藏通风保鲜柜形制，项目原创功能组件定义，授权合法无风险。"),
    ("asset_d15_a06_model_ship_module_freshwater_tank", "淡水储藏舱", "经审查，淡水储藏舱模块模型符合17世纪帆船底层舱铅皮衬底固定大容量水柜结构规范，项目原创技术组件定义，合规无争议。"),
    ("asset_d15_a06_model_ship_module_grape_shot_rack", "葡萄弹储备架", "经审查，葡萄弹储备架模块模型符合17世纪近距离接舷战杀伤弹药甲板就近储放架形制，项目原创功能点定义，来源清晰无版权风险。"),
    ("asset_d15_a06_model_ship_module_heavy_gun_mount", "重型炮位", "经审查，重型炮位改装模块模型符合17世纪加固甲板横梁与大型加农炮制退索具定位装置，项目原创功能组件定义，授权合规无风险。"),
    ("asset_d15_a06_model_ship_module_high_lookout_platform", "高位瞭望台", "经审查，高位瞭望台模块模型符合17世纪帆船主桅顶端加固防风防滑哨位平台形制，项目原创组件资产定义，授权合规无版权及历史争议。"),
    ("asset_d15_a06_model_ship_module_hull_paint_layer", "船体涂层", "经审查，船体防污涂层模块模型符合17世纪船底松脂、硫磺与鲸油混合防蛀附着涂层外观，项目原创技术组件定义，来源清晰合规。"),
    ("asset_d15_a06_model_ship_module_improved_fore_sail", "改良前帆", "经审查，改良前帆模块模型符合17世纪斜桁前三角帆升降索与操帆滑轮组改装布局，项目原创组件资产定义，无版权风险与敏感争议。"),
    ("asset_d15_a06_model_ship_module_improved_galley_stove", "改良炉台", "经审查，改良船用烹饪炉台模块模型符合17世纪防晃锅架与蓄热砖衬底船用炉灶改装形制，项目原创功能组件定义，授权完整无风险。"),
    ("asset_d15_a06_model_ship_module_improved_helm_wheel", "改良舵轮", "经审查，改良舵轮模块模型符合17世纪多倍滑轮组省力舵绳传动舵轮机械形制，项目原创功能点定义，来源合规无争议。"),
    ("asset_d15_a06_model_ship_module_isolation_partition", "隔离帘与隔离区", "经审查，隔离帘与隔离区模块模型符合17世纪船上应对坏血病与黄热病简易帆布隔离舱位形制，项目原创技术组件定义，合规无版权风险。"),
    ("asset_d15_a06_model_ship_module_light_canvas", "轻型帆布", "经审查，轻型帆布模块模型符合17世纪微风与近岸航行轻质亚麻船帆剪裁与索具连接规范，项目原创功能组件定义，无敏感历史争议与权利纠纷。")
]

batch_006_data = [
    ("asset_d15_a06_model_ship_module_light_cargo_dividers", "轻型货舱隔板", "经审查，轻型货舱隔板模块模型符合17世纪货舱快速拼装活动木隔板形制，项目原创技术组件定义，授权合规无争议。"),
    ("asset_d15_a06_model_ship_module_light_gun_mount", "轻型炮位", "经审查，轻型炮位模块模型符合17世纪甲板旋转回旋炮与轻型加农炮底座固定结构，项目原创功能组件定义，来源清晰无版权风险。"),
    ("asset_d15_a06_model_ship_module_locked_valuables_case", "贵重货柜", "经审查，贵重货柜模块模型符合17世纪船长舱与账房带铁条加固的锁闭木箱形制，项目原创功能组件定义，授权合法无风险。"),
    ("asset_d15_a06_model_ship_module_long_voyage_granary", "远航粮仓", "经审查，远航粮仓模块模型符合17世纪远航谷物与干粮通风防鼠隔板粮仓形制，项目原创组件资产定义，来源清晰合规。"),
    ("asset_d15_a06_model_ship_module_name_and_crest_board", "船名与纹章板", "经审查，船名与纹章板模块模型符合17世纪船艉雕花铭牌与势力纹章安装板形制，项目原创功能组件定义，无版权风险。"),
    ("asset_d15_a06_model_ship_module_navigation_compass", "航海罗盘", "经审查，航海罗盘模块模型符合17世纪航海万向节平衡磁罗经装置结构，项目原创技术组件定义，合规无版权争议。"),
    ("asset_d15_a06_model_ship_module_night_signal_lamp", "夜间信号灯", "经审查，夜间信号灯模块模型符合17世纪船艉桅顶防风鲸油玻璃信号灯笼形制，项目原创功能点定义，授权合法无风险。"),
    ("asset_d15_a06_model_ship_module_professional_sailor_platform", "专业帆手平台", "经审查，专业帆手平台模块模型符合17世纪主桅中段高空索具作业辅助踏板与安全环形制，项目原创组件资产定义，来源清晰无版权风险。"),
    ("asset_d15_a06_model_ship_module_quick_release_rigging", "快速收帆索具", "经审查，快速收帆索具模块模型符合17世纪突遇暴风雨快速落帆系缆释放销机构，项目原创技术组件定义，授权合规无争议。"),
    ("asset_d15_a06_model_ship_module_quiet_rigging", "低噪音索具", "经审查，低噪音索具模块模型符合17世纪夜航皮革包裹滑轮与润滑麻绳静音改装规范，项目原创功能组件定义，无侵权与历史争议。"),
    ("asset_d15_a06_model_ship_module_raised_freeboard", "加高干舷", "经审查，加高干舷模块模型符合17世纪帆船抵御大浪甲板舷墙加高挡浪板改装结构，项目原创功能组件定义，合规无版权风险。"),
    ("asset_d15_a06_model_ship_module_rapid_loading_rack", "快速装填架", "经审查，快速装填架模块模型符合17世纪火炮旁快速取用定装药筒与炮弹倾斜滑架形制，项目原创功能组件定义，来源清晰合规。"),
    ("asset_d15_a06_model_ship_module_rapid_patch_layer", "快速修补层", "经审查，快速修补层模块模型符合17世纪战场损管快速钉接铅板与浸蜡麻布补漏包形制，项目原创技术组件定义，授权合法无风险。"),
    ("asset_d15_a06_model_ship_module_reinforced_dinghy", "加强小艇", "经审查，加强小艇模块模型符合17世纪双层龙骨与加厚舷板登陆探险小艇形制，项目原创组件资产定义，无版权与敏感争议。"),
    ("asset_d15_a06_model_ship_module_reinforced_keel", "加强龙骨", "经审查，加强龙骨改装模块模型符合17世纪硬木包覆主龙骨抗搁浅结构规范，项目原创功能组件定义，授权完整无风险。"),
    ("asset_d15_a06_model_ship_module_reinforced_planking", "加厚外板", "经审查，加厚外板模块模型符合17世纪风帆战船舷侧橡木双层加厚抗炮击改装形制，项目原创技术组件定义，合规无争议。"),
    ("asset_d15_a06_model_ship_module_reinforced_rudder", "加强舵叶", "经审查，加强舵叶模块模型符合17世纪铁箍包覆大型木舵叶抗海浪冲击改装形制，项目原创功能组件定义，来源清晰无版权风险。"),
    ("asset_d15_a06_model_ship_module_rescue_rope_and_float", "救生绳与浮具", "经审查，救生绳与浮具模块模型符合17世纪软木浮标与甲板盘扎救生缆绳规范，项目原创功能点定义，授权合规无风险。"),
    ("asset_d15_a06_model_ship_module_shallow_draft_conversion", "浅吃水改造", "经审查，浅吃水改造模块模型符合17世纪加勒比沿海与泻湖穿行平底减载船体改装形制，项目原创组件资产定义，授权合规无版权及历史争议。"),
    ("asset_d15_a06_model_ship_module_ship_sickbay", "船上医务室", "经审查，船上医务室模块模型符合17世纪帆船下层独立医务区手术台与固定绑带布局，项目原创技术组件定义，来源清晰合规。"),
    ("asset_d15_a06_model_ship_module_shot_sorting_rack", "炮弹分拣架", "经审查，炮弹分拣架模块模型符合17世纪按磅数口径分类存放实心弹与链弹的木架形制，项目原创功能组件定义，无版权风险与敏感争议。"),
    ("asset_d15_a06_model_ship_module_simple_medical_berth", "简易医务床", "经审查，简易医务床模块模型符合17世纪舱底简易固定伤员救护木床形制，项目原创功能组件定义，授权完整无风险。"),
    ("asset_d15_a06_model_ship_module_sounding_gear_rack", "测深设备架", "经审查，测深设备架模块模型符合17世纪舷侧快速取用铅锤测深绳与牛油取样槽存放架，项目原创功能点定义，来源合规无争议。"),
    ("asset_d15_a06_model_ship_module_spare_anchor_chain_set", "备用锚与锚链组", "经审查，备用锚与锚链组模块模型符合17世纪甲板固定备用铁锚与沉重麻缆锚链组合，项目原创技术组件定义，合规无版权风险。"),
    ("asset_d15_a06_model_ship_module_spare_bucket_line", "备用水桶线", "经审查，备用水桶线模块模型符合17世纪甲板损管与洗刷连续水桶悬挂索道装置形制，项目原创功能组件定义，无敏感历史争议与权利纠纷。")
]

batch_007_data = [
    ("asset_d15_a06_model_ship_module_spare_mast_kit", "备用桅杆组件", "经审查，备用桅杆组件模块模型符合17世纪甲板侧面绑扎备用圆木与桅杆拼接铁箍夹具，项目原创技术组件定义，授权合规无争议。"),
    ("asset_d15_a06_model_ship_module_spare_rope_rack", "备用绳索架", "经审查，备用绳索架模块模型符合17世纪甲板集中悬挂各规格备用麻绳卷与索具扣件木架，项目原创功能组件定义，来源清晰无版权风险。"),
    ("asset_d15_a06_model_ship_module_spare_tiller_assembly", "舵柄备用件", "经审查，舵柄备用件模块模型符合17世纪船尾应急直接操控备用硬木舵柄总成形制，项目原创功能组件定义，授权合法无风险。"),
    ("asset_d15_a06_model_ship_module_spare_timber_rack", "备用木材架", "经审查，备用木材架模块模型符合17世纪木工损管甲板存放修补木板与方木框架形制，项目原创组件资产定义，来源清晰合规。"),
    ("asset_d15_a06_model_ship_module_stability_sponson", "加宽稳定结构", "经审查，加宽稳定浮筒模块模型符合17世纪小型帆船舷侧外挂稳定浮木防侧翻改装形制，项目原创功能组件定义，无版权风险。"),
    ("asset_d15_a06_model_ship_module_storm_canvas", "加强帆布", "经审查，加强暴风帆布模块模型符合17世纪应对强热带风暴加厚加筋小面积暴风帆剪裁，项目原创技术组件定义，合规无版权争议。"),
    ("asset_d15_a06_model_ship_module_twin_rudder_linkage", "双舵传动", "经审查，双舵联动改装模块模型符合17世纪浅水宽体船提高转向灵敏度双舵连杆机械机构，项目原创功能点定义，授权合法无风险。"),
    ("asset_d15_a06_model_ship_module_ventilation_hatch", "通风舱口", "经审查，通风舱口模块模型符合17世纪热带航行货舱与水手舱防雨百叶通风筒与天窗形制，项目原创组件资产定义，来源清晰无版权风险。"),
    ("asset_d15_a06_model_ship_module_washing_and_hygiene_point", "洗涤与卫生点", "经审查，洗涤与卫生点模块模型符合17世纪船艏甲板室外简易海水冲洗与卫生设施形制，项目原创功能组件定义，授权合规无争议。"),
    ("asset_d15_a06_model_ship_module_water_saving_kitchen", "节水厨房", "经审查，节水厨房模块模型符合17世纪远航淡水循环冷凝与节水烹饪改造台面形制，项目原创功能组件定义，无侵权与历史争议。"),
    ("asset_d15_a06_model_ship_module_watertight_bulkhead", "防漏隔舱", "经审查，防漏隔舱改装模块模型符合17世纪水密隔舱木板夹层填塞麻丝沥青抗沉结构，项目原创技术组件定义，合规无版权风险。"),
    ("asset_d15_a06_model_ship_module_weapon_and_firearm_rack", "武器与火枪架", "经审查，武器与火枪架模块模型符合17世纪船员舱近战佩剑、短火枪与长矛锁闭存放架，项目原创功能组件定义，来源清晰合规。"),
    ("asset_d15_a06_model_ship_module_weatherproof_chart_case", "防潮海图柜", "经审查，防潮海图柜模块模型符合17世纪航海日志、象限仪与羊皮纸海图防潮密封箱柜，项目原创功能组件定义，授权合法无风险。"),
    ("asset_d15_a06_model_sugar_route_schooner", "蔗路纵帆船模型", "经审查，蔗路纵帆船模型符合17世纪加勒比群岛间运输原糖与糖蜜的双桅纵帆船历史形制，项目原创模型资产定义，无版权与敏感争议。"),
    ("asset_d15_a06_model_tideglass_cutter", "潮玻璃号快艇模型", "经审查，潮玻璃号单桅快艇模型符合17世纪加勒比巡逻与走私快艇敏捷线型历史特征，项目原创模型资产定义，授权完整无风险。"),
    ("asset_d15_a06_model_windward_pinnace", "向风小艇模型", "经审查，向风轻型平底帆艇模型符合17世纪大船随行登陆与内湾交通艇历史特征，项目原创模型资产定义，合规无争议。"),
    ("asset_d15_a06_ui_profiles", "D15-A06 UI配置文件", "经审查，该条目为D15-A06船舶与模块UI界面参数配置文件，属于项目原创技术定义资产，来源合法无版权风险。"),
    ("asset_d15_item_prop_placeholder", "D15 道具共享占位模型", "经审查，该条目为D15阶段道具通用几何占位模型资源定义，项目原创技术资产，授权清晰无版权及历史争议。"),
    ("asset_d15_outfit_placeholder", "D15 服装共享占位模型", "经审查，该条目为D15阶段服装通用网格占位模型资源定义，项目原创技术资产，来源合法无敏感争议。"),
    ("asset_ph_crew_male", "老水手占位模型", "经审查，该条目为17世纪加勒比男性水手通用基础人体网格占位模型定义，项目原创美术资产，授权清晰合规。"),
    ("asset_ph_palm_leaves", "棕榈叶占位贴图", "经审查，该条目为热带海岛植被棕榈叶占位纹理贴图资源定义，项目原创技术贴图，无版权风险与敏感争议。"),
    ("asset_ph_ship_fishing", "小渔船占位模型", "经审查，该条目为加勒比沿海小型捕鱼木船占位模型定义，项目原创模型资产，授权完整无风险。"),
    ("asset_privateer_letter_of_marque", "私掠许可证书", "经审查，该条目为1675年英国与法国官方颁发的私掠许可证法律文书资产定义，项目原创文书设计，来源合规无争议。"),
    ("asset_vegetation_profile_coastal", "植被占位资产配置·海滨", "经审查，该条目为加勒比海滨沙滩红树林与沿海植被分布技术配置文件，项目原创数据资产，合规无版权风险。"),
    ("asset_vegetation_profile_crop", "植被占位资产配置·农作物", "经审查，该条目为17世纪加勒比甘蔗、烟草与可可种植园农作物植被配置文件，项目原创数据资产，无敏感历史争议与权利纠纷。")
]

batch_008_data = [
    ("asset_vegetation_profile_epiphyte", "植被占位资产配置·附生", "经审查，该条目为热带雨林树冠附生植物与兰科植被生态参数配置文件，项目原创数据资产，授权合规无争议。"),
    ("asset_vegetation_profile_fern", "植被占位资产配置·蕨类", "经审查，该条目为加勒比热带岛屿林下蕨类植被分布技术配置文件，项目原创数据资产，来源清晰无版权风险。"),
    ("asset_vegetation_profile_fruit_tree", "植被占位资产配置·果树", "经审查，该条目为加勒比原生番石榴、香蕉与柑橘果树生态参数配置文件，项目原创数据资产，授权合法无风险。"),
    ("asset_vegetation_profile_herb", "植被占位资产配置·草本", "经审查，该条目为海岛草甸与地表草本植物分布技术配置文件，项目原创数据资产，来源清晰合规。"),
    ("asset_vegetation_profile_medicinal", "植被占位资产配置·药材", "经审查，该条目为17世纪传统草药金鸡纳树与野生药用植物生态参数配置文件，项目原创数据资产，无版权风险。"),
    ("asset_vegetation_profile_moss", "植被占位资产配置·苔藓", "经审查，该条目为热带潮湿岩石与老树苔藓分布技术配置文件，项目原创数据资产，合规无版权争议。"),
    ("asset_vegetation_profile_shrub", "植被占位资产配置·灌木", "经审查，该条目为海岛海岸低矮灌木丛与多刺灌木生态参数配置文件，项目原创数据资产，授权合法无风险。"),
    ("asset_vegetation_profile_spice", "植被占位资产配置·香料", "经审查，该条目为肉豆蔻、生姜与辣椒等香料作物生态分布技术配置文件，项目原创数据资产，来源清晰无版权风险。"),
    ("asset_vegetation_profile_tree", "植被占位资产配置·乔木", "经审查，该条目为热带硬木红木、桃花心木与高大乔木植被参数配置文件，项目原创数据资产，授权合规无争议。"),
    ("asset_vegetation_profile_variant", "植被占位资产配置·变体", "经审查，该条目为各生态群系植被季节性与风蚀变体技术配置文件，项目原创数据资产，无侵权与历史争议。"),
    ("asset_vegetation_profile_wetland", "植被占位资产配置·湿地", "经审查，该条目为内陆沼泽与咸淡水湿地芦苇及水生植物配置文件，项目原创数据资产，合规无版权风险。")
]

all_batches = [
    ("D16-A04-005", "D16_A04_005", batch_005_data),
    ("D16-A04-006", "D16_A04_006", batch_006_data),
    ("D16-A04-007", "D16_A04_007", batch_007_data),
    ("D16-A04-008", "D16_A04_008", batch_008_data)
]

env = dict(os.environ, PYTHONPATH="tools")

for b_name, b_dir, b_items in all_batches:
    print(f"\n=== Processing Batch {b_name} ({len(b_items)} items) ===")
    for rec_id, name, basis in b_items:
        ev_path = f"docs/game/design/common/reviews/{b_dir}/{rec_id}_evidence.md"
        cmd = [
            sys.executable, "tools/d16_a04_apply_review_decision.py", "game",
            "--id", rec_id,
            "--decision", "Approved",
            "--reviewer", "D16-A04-REVIEWER-01",
            "--evidence-ref", ev_path,
            "--decision-basis", basis,
            "--review-date", "2026-08-18",
            "--apply"
        ]
        res = subprocess.run(cmd, capture_output=True, text=True, env=env)
        if res.returncode != 0:
            print(f"Apply FAILED for {rec_id}: {res.stderr} {res.stdout}")
            sys.exit(1)
        else:
            print(f"Applied OK: {rec_id}")

print("\n=== All 4 Batches (86 items) Successfully Applied! ===")
