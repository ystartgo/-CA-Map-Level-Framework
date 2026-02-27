\[MLF] Patched SectionLayer\_SunShadows.Regenerate OK.

\[MLF] Patched SectionLayer\_SunShadows.DrawLayer OK.

\[MLF] Patched SectionLayer\_Zones.Regenerate OK.

\[MLF] Initialized.

【MLF】FindStairsToFloor: elev=0→1, 结果=(161, 0, 87)

【MLF】寻路与job检测-珊瑚花—本层有工作(pri=2, Clean)，但2F有更高优先级工作(UniversalMaterial@(161, 0, 78))→跨层

【MLF】寻路与job检测-珊瑚花—执行UseStairs: 1F→2F

\[MLF] Transferred 珊瑚花 to map 1 at (161, 0, 87)

【MLF】FindStairsToFloor: elev=1→0, 结果=(161, 0, 87)

【MLF】寻路与job检测-珊瑚花—到达意图目标层2F，执行原版job: MLF\_HaulAcrossLevel

\[MLF] Transferred 珊瑚花 to map 0 at (161, 0, 87)

【MLF】FindStairsToFloor: elev=0→1, 结果=(161, 0, 87)

【MLF】寻路与job检测-铁兰花—本层有工作(pri=2, Clean)，但2F有更高优先级工作(UniversalMaterial@(161, 0, 78))→跨层

【MLF】寻路与job检测-铁兰花—执行UseStairs: 1F→2F

\[MLF] Transferred 铁兰花 to map 1 at (161, 0, 87)

【MLF】FindStairsToFloor: elev=1→0, 结果=(161, 0, 87)

【MLF】寻路与job检测-铁兰花—到达意图目标层2F，执行原版job: MLF\_HaulAcrossLevel

\[MLF] Transferred 铁兰花 to map 0 at (161, 0, 87)

【MLF】FindStairsToFloor: elev=0→1, 结果=(161, 0, 87)

【MLF】寻路与job检测-铁兰花—本层有工作(pri=2, Clean)，但2F有更高优先级工作(RK\_ElectricSmithy@(161, 0, 77))→跨层

【MLF】寻路与job检测-铁兰花—执行UseStairs: 1F→2F

\[MLF] Transferred 铁兰花 to map 1 at (161, 0, 87)

【MLF】寻路与job检测-铁兰花—到达意图目标层2F，执行原版job: Research

【MLF】寻路与job检测-珊瑚花—在1F，本层无工作，开始跨层扫描

【MLF】寻路与job检测-珊瑚花—  P1-Bill ing\[钢铁]x60: 2F不够, 1F找到=Steel

【MLF】寻路与job检测-珊瑚花—  P1-Bill ing\[金属]x100: 2F不够, 1F找到=UniversalMaterial

【MLF】FindStairsToFloor: elev=0→1, 结果=(161, 0, 87)

【MLF】寻路与job检测-珊瑚花—  P1-Bill编码: workbench=RK\_ElectricSmithy@(161, 0, 77), size=(2, 1), encoded=(1,161,77)

【MLF】寻路与job检测-珊瑚花—  P1-Bill命中(多材料): 搬钢铁x109+精粹x109→2F 给精粹鼠族锻造台（电力）

【MLF】寻路与job检测-珊瑚花—P1命中→封装投递 

【MLF】跨层投递-珊瑚花—拾取到背包: 钢铁x109

【MLF】跨层投递-珊瑚花—拾取到背包: 精粹x109

\[MLF] Focus switched: 1 -> 0

【MLF】跨层投递-珊瑚花—多材料传送: →1, 背包\[钢铁x109+精粹x109]

\[MLF] Transferred 珊瑚花 to map 1 at (161, 0, 87)

【MLF】跨层投递-珊瑚花—FindBillGiverNear((161, 0, 77)): 精粹鼠族锻造台（电力）@(161, 0, 77)

【MLF】跨层投递-珊瑚花—丢下\[1]: 精粹x109, inInventory=True

【MLF】跨层投递-珊瑚花—丢下OK: 精粹x109@(161, 0, 78), Forbid it

【MLF】跨层投递-珊瑚花—丢下\[0]: 钢铁x109, inInventory=True

【MLF】跨层投递-珊瑚花—丢下OK: 钢铁x109@(160, 0, 78), Forbid it

【MLF】跨层投递-珊瑚花—Bill\[0]: Make\_RK\_PlateHelmB, ShouldDoNow=False, PawnAllowed=True, ingredients=\[x40] 

【MLF】跨层投递-珊瑚花—Bill\[1]: Make\_RK\_Plate, ShouldDoNow=True, PawnAllowed=True, ingredients=\[金属x100] \[钢铁x60] 

【MLF】跨层投递-珊瑚花—Bill\[2]: Make\_RK\_HeavyShield\_Big, ShouldDoNow=True, PawnAllowed=True, ingredients=\[金属x165] \[钢铁x70] 

【MLF】跨层投递-珊瑚花—Bill\[3]: Make\_RK\_OneHanded, ShouldDoNow=True, PawnAllowed=True, ingredients=\[金属x90] \[原木x25] 

【MLF】TryMatch—ing\[0]: filter=钢铁, needed=60

【MLF】TryMatch—  dropped\[0] UniversalMaterialx109: filterAllow=False, userFilter=True, billAllow=True

【MLF】TryMatch—  dropped\[1] Steelx109: valuePerUnit=1

【MLF】TryMatch—  → 使用 Steelx60, 剩余needed=0

【MLF】TryMatch—ing\[1]: filter=金属, needed=100

【MLF】TryMatch—  dropped\[0] UniversalMaterialx109: valuePerUnit=1

【MLF】TryMatch—  → 使用 UniversalMaterialx100, 剩余needed=0

【MLF】TryMatch—全部匹配成功! chosen=2个

【MLF】跨层投递-珊瑚花—直接DoBill成功: 精粹鼠族锻造台（电力）, Unforbid 2个材料

【MLF】FindStairsToFloor: elev=1→0, 结果=(161, 0, 87)

\[MLF] Transferred 珊瑚花 to map 0 at (161, 0, 87)

\[MLF] Focus switched: 0 -> 1

【MLF】寻路与job检测-珊瑚花—在1F，本层无工作，开始跨层扫描

【MLF】寻路与job检测-珊瑚花—  P1-Bill ing\[钢铁]x70: 2F不够, 1F找到=Steel

【MLF】寻路与job检测-珊瑚花—  P1-Bill ing\[金属]x165: 2F不够, 1F找到=UniversalMaterial

【MLF】FindStairsToFloor: elev=0→1, 结果=(161, 0, 87)

【MLF】寻路与job检测-珊瑚花—  P1-Bill编码: workbench=RK\_ElectricSmithy@(161, 0, 77), size=(2, 1), encoded=(1,161,77)

【MLF】寻路与job检测-珊瑚花—  P1-Bill命中(多材料): 搬钢铁x109+精粹x109→2F 给精粹鼠族锻造台（电力）

【MLF】寻路与job检测-珊瑚花—P1命中→封装投递 

【MLF】跨层投递-珊瑚花—拾取到背包: 钢铁x109

【MLF】跨层投递-珊瑚花—拾取到背包: 精粹x109

【MLF】跨层投递-珊瑚花—多材料传送: →1, 背包\[钢铁x109+精粹x109]

\[MLF] Transferred 珊瑚花 to map 1 at (161, 0, 87)

【MLF】跨层投递-珊瑚花—FindBillGiverNear((161, 0, 77)): 精粹鼠族锻造台（电力）@(161, 0, 77)

【MLF】跨层投递-珊瑚花—丢下\[1]: 精粹x109, inInventory=True

【MLF】跨层投递-珊瑚花—丢下OK: 精粹x118@(161, 0, 78), Forbid it

【MLF】跨层投递-珊瑚花—丢下\[0]: 钢铁x109, inInventory=True

【MLF】跨层投递-珊瑚花—丢下OK: 钢铁x109@(160, 0, 78), Forbid it

【MLF】跨层投递-珊瑚花—Bill\[0]: Make\_RK\_PlateHelmB, ShouldDoNow=False, PawnAllowed=True, ingredients=\[x40] 

【MLF】跨层投递-珊瑚花—Bill\[1]: Make\_RK\_Plate, ShouldDoNow=False, PawnAllowed=True, ingredients=\[金属x100] \[钢铁x60] 

【MLF】跨层投递-珊瑚花—Bill\[2]: Make\_RK\_HeavyShield\_Big, ShouldDoNow=True, PawnAllowed=True, ingredients=\[金属x165] \[钢铁x70] 

【MLF】跨层投递-珊瑚花—Bill\[3]: Make\_RK\_OneHanded, ShouldDoNow=True, PawnAllowed=True, ingredients=\[金属x90] \[原木x25] 

【MLF】TryMatch—ing\[0]: filter=钢铁, needed=70

【MLF】TryMatch—  dropped\[0] UniversalMaterialx118: filterAllow=False, userFilter=True, billAllow=True

【MLF】TryMatch—  dropped\[1] Steelx109: valuePerUnit=1

【MLF】TryMatch—  → 使用 Steelx70, 剩余needed=0

【MLF】TryMatch—ing\[1]: filter=金属, needed=165

【MLF】TryMatch—  dropped\[0] UniversalMaterialx118: valuePerUnit=1

【MLF】TryMatch—  → 使用 UniversalMaterialx118, 剩余needed=47

【MLF】TryMatch—  dropped\[1] Steelx109: filterAllow=True, userFilter=False, billAllow=True

【MLF】TryMatch—  dropped不够，搜索工作台附近...

【MLF】TryMatch—  工作台附近找到0个，仍不够: needed=47

【MLF】TryMatch—ing\[1] 失败: 金属 缺 47

【MLF】TryMatch—ing\[0]: filter=原木, needed=25

【MLF】TryMatch—  dropped\[0] UniversalMaterialx118: filterAllow=False, userFilter=True, billAllow=True

【MLF】TryMatch—  dropped\[1] Steelx109: filterAllow=False, userFilter=True, billAllow=False

【MLF】TryMatch—  dropped不够，搜索工作台附近...

【MLF】TryMatch—  工作台附近找到0个，仍不够: needed=25

【MLF】TryMatch—ing\[0] 失败: 原木 缺 25

【MLF】跨层投递-珊瑚花—直接DoBill失败，尝试原版搜索...

【MLF】跨层投递-珊瑚花—DoBill全部失败，材料已Forbid等待下趟

【MLF】FindStairsToFloor: elev=1→0, 结果=(161, 0, 87)

\[MLF] Transferred 珊瑚花 to map 0 at (161, 0, 87)

【MLF】FindStairsToFloor: elev=1→0, 结果=(161, 0, 87)

【MLF】寻路与job检测-铁兰花—本层有工作(pri=3, Research)，但1F有更高优先级工作(CraftingTable@(154, 0, 87))→跨层

【MLF】寻路与job检测-铁兰花—执行UseStairs: 2F→1F

\[MLF] Transferred 铁兰花 to map 0 at (161, 0, 87)

【MLF】寻路与job检测-铁兰花—到达意图目标层1F，但原版无job，继续扫描

【MLF】寻路与job检测-铁兰花—在1F，本层无工作，开始跨层扫描

【MLF】寻路与job检测-铁兰花—  P1-Bill ing\[钢铁]x70: 2F不够, 1F找到=Steel

【MLF】寻路与job检测-铁兰花—  P1-Bill ing\[金属]x165: 2F不够, 1F找到=UniversalMaterial

【MLF】FindStairsToFloor: elev=0→1, 结果=(161, 0, 87)

【MLF】寻路与job检测-铁兰花—  P1-Bill编码: workbench=RK\_ElectricSmithy@(161, 0, 77), size=(2, 1), encoded=(1,161,77)

【MLF】寻路与job检测-铁兰花—  P1-Bill命中(多材料): 搬钢铁x72+精粹x72→2F 给精粹鼠族锻造台（电力）

【MLF】寻路与job检测-铁兰花—P1命中→封装投递 

【MLF】跨层投递-铁兰花—拾取到背包: 钢铁x72

【MLF】跨层投递-铁兰花—拾取到背包: 精粹x72

【MLF】跨层投递-铁兰花—多材料传送: →1, 背包\[钢铁x72+精粹x72]

\[MLF] Transferred 铁兰花 to map 1 at (161, 0, 87)

【MLF】跨层投递-铁兰花—FindBillGiverNear((161, 0, 77)): 精粹鼠族锻造台（电力）@(161, 0, 77)

【MLF】跨层投递-铁兰花—丢下\[1]: 精粹x72, inInventory=True

【MLF】跨层投递-铁兰花—丢下OK: 精粹x190@(161, 0, 78), Forbid it

【MLF】跨层投递-铁兰花—丢下\[0]: 钢铁x72, inInventory=True

【MLF】跨层投递-铁兰花—丢下OK: 钢铁x181@(160, 0, 78), Forbid it

【MLF】跨层投递-铁兰花—Bill\[0]: Make\_RK\_PlateHelmB, ShouldDoNow=False, PawnAllowed=True, ingredients=\[x40] 

【MLF】跨层投递-铁兰花—Bill\[1]: Make\_RK\_Plate, ShouldDoNow=False, PawnAllowed=True, ingredients=\[金属x100] \[钢铁x60] 

【MLF】跨层投递-铁兰花—Bill\[2]: Make\_RK\_HeavyShield\_Big, ShouldDoNow=True, PawnAllowed=True, ingredients=\[金属x165] \[钢铁x70] 

【MLF】跨层投递-铁兰花—Bill\[3]: Make\_RK\_OneHanded, ShouldDoNow=True, PawnAllowed=True, ingredients=\[金属x90] \[原木x25] 

【MLF】TryMatch—ing\[0]: filter=钢铁, needed=70

【MLF】TryMatch—  dropped\[0] UniversalMaterialx190: filterAllow=False, userFilter=True, billAllow=True

【MLF】TryMatch—  dropped\[1] Steelx181: valuePerUnit=1

【MLF】TryMatch—  → 使用 Steelx70, 剩余needed=0

【MLF】TryMatch—ing\[1]: filter=金属, needed=165

【MLF】TryMatch—  dropped\[0] UniversalMaterialx190: valuePerUnit=1

【MLF】TryMatch—  → 使用 UniversalMaterialx165, 剩余needed=0

【MLF】TryMatch—全部匹配成功! chosen=2个

【MLF】跨层投递-铁兰花—直接DoBill成功: 精粹鼠族锻造台（电力）, Unforbid 2个材料

【MLF】FindStairsToFloor: elev=1→0, 结果=(161, 0, 87)

\[MLF] Transferred 铁兰花 to map 0 at (161, 0, 87)

【MLF】寻路与job检测-铁兰花—在1F，本层无工作，开始跨层扫描

【MLF】寻路与job检测-铁兰花—  P1-Bill ing\[钢铁]x60: 2F不够, 1F找到=Steel

【MLF】寻路与job检测-铁兰花—  P1-Bill ing\[金属]x100: 2F不够, 1F找到=UniversalMaterial

【MLF】FindStairsToFloor: elev=0→1, 结果=(161, 0, 87)

【MLF】寻路与job检测-铁兰花—  P1-Bill编码: workbench=RK\_ElectricSmithy@(161, 0, 77), size=(2, 1), encoded=(1,161,77)

【MLF】寻路与job检测-铁兰花—  P1-Bill命中(多材料): 搬钢铁x72+精粹x72→2F 给精粹鼠族锻造台（电力）

【MLF】寻路与job检测-铁兰花—P1命中→封装投递 

【MLF】跨层投递-铁兰花—拾取到背包: 钢铁x72

【MLF】跨层投递-铁兰花—拾取到背包: 精粹x72

【MLF】跨层投递-铁兰花—多材料传送: →1, 背包\[钢铁x72+精粹x72]

\[MLF] Transferred 铁兰花 to map 1 at (161, 0, 87)

【MLF】跨层投递-铁兰花—FindBillGiverNear((161, 0, 77)): 精粹鼠族锻造台（电力）@(161, 0, 77)

【MLF】跨层投递-铁兰花—丢下\[1]: 精粹x72, inInventory=True

【MLF】跨层投递-铁兰花—丢下OK: 精粹x97@(161, 0, 78), Forbid it

【MLF】跨层投递-铁兰花—丢下\[0]: 钢铁x72, inInventory=True

【MLF】跨层投递-铁兰花—丢下OK: 钢铁x111@(160, 0, 78), Forbid it

【MLF】跨层投递-铁兰花—Bill\[0]: Make\_RK\_PlateHelmB, ShouldDoNow=False, PawnAllowed=True, ingredients=\[x40] 

【MLF】跨层投递-铁兰花—Bill\[1]: Make\_RK\_Plate, ShouldDoNow=True, PawnAllowed=True, ingredients=\[金属x100] \[钢铁x60] 

【MLF】跨层投递-铁兰花—Bill\[2]: Make\_RK\_HeavyShield\_Big, ShouldDoNow=True, PawnAllowed=True, ingredients=\[金属x165] \[钢铁x70] 

【MLF】跨层投递-铁兰花—Bill\[3]: Make\_RK\_OneHanded, ShouldDoNow=True, PawnAllowed=True, ingredients=\[金属x90] \[原木x25] 

【MLF】TryMatch—ing\[0]: filter=钢铁, needed=60

【MLF】TryMatch—  dropped\[0] UniversalMaterialx97: filterAllow=False, userFilter=True, billAllow=True

【MLF】TryMatch—  dropped\[1] Steelx111: valuePerUnit=1

【MLF】TryMatch—  → 使用 Steelx60, 剩余needed=0

【MLF】TryMatch—ing\[1]: filter=金属, needed=100

【MLF】TryMatch—  dropped\[0] UniversalMaterialx97: valuePerUnit=1

【MLF】TryMatch—  → 使用 UniversalMaterialx97, 剩余needed=3

【MLF】TryMatch—  dropped\[1] Steelx111: filterAllow=True, userFilter=False, billAllow=True

【MLF】TryMatch—  dropped不够，搜索工作台附近...

【MLF】TryMatch—  工作台附近找到0个，仍不够: needed=3

【MLF】TryMatch—ing\[1] 失败: 金属 缺 3

【MLF】TryMatch—ing\[0]: filter=钢铁, needed=70

【MLF】TryMatch—  dropped\[0] UniversalMaterialx97: filterAllow=False, userFilter=True, billAllow=True

【MLF】TryMatch—  dropped\[1] Steelx111: valuePerUnit=1

【MLF】TryMatch—  → 使用 Steelx70, 剩余needed=0

【MLF】TryMatch—ing\[1]: filter=金属, needed=165

【MLF】TryMatch—  dropped\[0] UniversalMaterialx97: valuePerUnit=1

【MLF】TryMatch—  → 使用 UniversalMaterialx97, 剩余needed=68

【MLF】TryMatch—  dropped\[1] Steelx111: filterAllow=True, userFilter=False, billAllow=True

【MLF】TryMatch—  dropped不够，搜索工作台附近...

【MLF】TryMatch—  工作台附近找到0个，仍不够: needed=68

【MLF】TryMatch—ing\[1] 失败: 金属 缺 68

【MLF】TryMatch—ing\[0]: filter=原木, needed=25

【MLF】TryMatch—  dropped\[0] UniversalMaterialx97: filterAllow=False, userFilter=True, billAllow=True

【MLF】TryMatch—  dropped\[1] Steelx111: filterAllow=False, userFilter=True, billAllow=False

【MLF】TryMatch—  dropped不够，搜索工作台附近...

【MLF】TryMatch—  工作台附近找到0个，仍不够: needed=25

【MLF】TryMatch—ing\[0] 失败: 原木 缺 25

【MLF】跨层投递-铁兰花—直接DoBill失败，尝试原版搜索...

【MLF】跨层投递-铁兰花—DoBill全部失败，材料已Forbid等待下趟

【MLF】FindStairsToFloor: elev=1→0, 结果=(161, 0, 87)

\[MLF] Transferred 铁兰花 to map 0 at (161, 0, 87)

【MLF】寻路与job检测-铁兰花—在1F，本层无工作，开始跨层扫描

【MLF】寻路与job检测-铁兰花—  P1-Bill ing\[钢铁]x60: 2F不够, 1F找到=Steel

【MLF】寻路与job检测-铁兰花—  P1-Bill ing\[金属]x100: 2F不够, 1F找到=UniversalMaterial

【MLF】FindStairsToFloor: elev=0→1, 结果=(161, 0, 87)

【MLF】寻路与job检测-铁兰花—  P1-Bill编码: workbench=RK\_ElectricSmithy@(161, 0, 77), size=(2, 1), encoded=(1,161,77)

【MLF】寻路与job检测-铁兰花—  P1-Bill命中(多材料): 搬钢铁x72+精粹x72→2F 给精粹鼠族锻造台（电力）

【MLF】寻路与job检测-铁兰花—P1命中→封装投递 

【MLF】跨层投递-铁兰花—拾取到背包: 钢铁x72

【MLF】跨层投递-铁兰花—拾取到背包: 精粹x72

【MLF】跨层投递-铁兰花—多材料传送: →1, 背包\[钢铁x72+精粹x72]

\[MLF] Transferred 铁兰花 to map 1 at (161, 0, 87)

【MLF】跨层投递-铁兰花—FindBillGiverNear((161, 0, 77)): 精粹鼠族锻造台（电力）@(161, 0, 77)

【MLF】跨层投递-铁兰花—丢下\[1]: 精粹x72, inInventory=True

【MLF】跨层投递-铁兰花—丢下OK: 精粹x169@(161, 0, 78), Forbid it

【MLF】跨层投递-铁兰花—丢下\[0]: 钢铁x72, inInventory=True

【MLF】跨层投递-铁兰花—丢下OK: 钢铁x183@(160, 0, 78), Forbid it

【MLF】跨层投递-铁兰花—Bill\[0]: Make\_RK\_PlateHelmB, ShouldDoNow=False, PawnAllowed=True, ingredients=\[x40] 

【MLF】跨层投递-铁兰花—Bill\[1]: Make\_RK\_Plate, ShouldDoNow=True, PawnAllowed=True, ingredients=\[金属x100] \[钢铁x60] 

【MLF】跨层投递-铁兰花—Bill\[2]: Make\_RK\_HeavyShield\_Big, ShouldDoNow=True, PawnAllowed=True, ingredients=\[金属x165] \[钢铁x70] 

【MLF】跨层投递-铁兰花—Bill\[3]: Make\_RK\_OneHanded, ShouldDoNow=True, PawnAllowed=True, ingredients=\[金属x90] \[原木x25] 

【MLF】TryMatch—ing\[0]: filter=钢铁, needed=60

【MLF】TryMatch—  dropped\[0] UniversalMaterialx169: filterAllow=False, userFilter=True, billAllow=True

【MLF】TryMatch—  dropped\[1] Steelx183: valuePerUnit=1

【MLF】TryMatch—  → 使用 Steelx60, 剩余needed=0

【MLF】TryMatch—ing\[1]: filter=金属, needed=100

【MLF】TryMatch—  dropped\[0] UniversalMaterialx169: valuePerUnit=1

【MLF】TryMatch—  → 使用 UniversalMaterialx100, 剩余needed=0

【MLF】TryMatch—全部匹配成功! chosen=2个

【MLF】跨层投递-铁兰花—直接DoBill成功: 精粹鼠族锻造台（电力）, Unforbid 2个材料

【MLF】FindStairsToFloor: elev=1→0, 结果=(161, 0, 87)

【MLF】寻路与job检测-铁兰花—需求跨层: 休息，原因: 自己的床在0F，去睡觉 (category=Tired, level=19 %)，当前楼层: 1F

【MLF】寻路与job检测-铁兰花—执行UseStairs: 2F→1F

\[MLF] Transferred 铁兰花 to map 0 at (161, 0, 87)

