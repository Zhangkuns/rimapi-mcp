# Core 炮塔/迫击炮/火箭蜂群与爆破（C-16）

2026-11-13 新增。目标/停火/弹药/引信全部走原生 API；不开 UI Targeter，
不用 DEV 瞬爆。

## 炮塔

`GET /api/v1/building/turret?thing_id=`
- `Building_TurretGun`：forced/current target、hold_fire、active、mannable/
  manned、cooldown/warmup、can_force/can_hold/can_extract、is_mortar/
  is_rocketswarm；非炮塔 422。
- Shell（`CompChangeableProjectile`）：loaded_shell/count/projectile、
  allowed_shells（ITab_Shells 同源 filter）。

`POST /api/v1/building/turret`
`action`: `force_target`（`target_pawn_id` 或 `target{x,z}` 二选一）、
`clear_target`、`hold_fire`（`enabled`，开启时清 forced target）、
`load_shell`（`shell_def`）、`eject_shell`（`can_extract_shell` 门禁）、
`allow_shell`（`enabled`）、`fire_rocketswarm`（显式落点）。
- 强制目标走原生 `OrderAttack`；shell `LoadShell`/`RemoveShell + GenPlace`；
  allow 走 `allowedShellsSettings.filter.SetAllow`。
- rocketswarm 校验 Active/目标/弹药/冷却后原生 `OrderAttack` +
  `TryActivateBurst`，不弹 UI。
- dry_run 不写 holdFire/目标/弹药/过滤、不激活；失败 4xx 不部分写。

## 爆破

`GET /api/v1/building/explosive?thing_id=`：wick_started/ticks_left/radius/
damage/destroyed；仅 `CompExplosive`。

`POST /api/v1/building/explosive`：`{thing_id,dry_run}` 只调原生
`CompExplosive.StartWick()`；已点引信 409；不调 DEV；真实倒计时/爆炸读回。
`Building_BlastingCharge` 无 Def 实例时用原生 CompExplosive 建筑（披露）。

## 证据与边界

2026-11-13 隔离 g35：fixture 放 MiniTurret/Mortar/Rocketswarm 于无顶混凝土地面，并用原生 `JobDefOf.ManTurret` 让殖民者上炮。
- MiniTurret 原生不可强瞄（can_force=false）→ force 422；hold fire 可切。
- Mortar：先装 `Shell_HighExplosive`，原生 `ManTurret` job 上炮
  （`MannedNow=true`），force/hold/clear/refire、退弹、filter
  disallow/allow 读回；`can_extract_shell` 由原生 manned/player-controlled
  门禁。
- Mortar 真实发射：装弹 + 上炮 + 强制目标后推进，shell 由 1→0、
  `burstCooldownTicksLeft=1680`、`current_target`/`forced_target` 有效
  （`real_fire_verified=true`）。
- Rocketswarm 显式落点 `fire_rocketswarm` 原生激活成功。
- Mortar `CompExplosive`：GET 未点引信；POST `StartWick()` applied；重复 409；
  有界推进后真实爆炸/`destroyed=true`。
- 4xx：mini force 422；pawn+cell 400；无目标 400；越界 422；非炮塔 422；
  未知 404。
- `desktop.py stop`：source_save/formal_dll/global_prefs unchanged。
- **未验**：rocketswarm 弹着伤害；`Building_BlastingCharge` 因本安装无
  Def 实例改用 Mortar CompExplosive。装载/退弹/过滤/强制目标/真实发射/
  冷却/引信均为真实原生读回。
