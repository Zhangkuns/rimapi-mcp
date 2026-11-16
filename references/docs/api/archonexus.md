# Quest choices and Archonexus

Maintainer: Codex; checked: 2026-11-11; status: implemented, validation scope recorded in the [execution plan](../exec-plans/active/five-dlc-remaining.md).

All paths below are relative to `/api/v1`. Reads and writes execute on the game main thread.
Writes support `dry_run` and optional `Idempotency-Key` with game-memory frozen replay;
keys have the [existing lifetime](idempotency.md). A submitted job, cinematic or queued map generation is not completion.

| Method / path | Input | Native operation / readback |
| --- | --- | --- |
| `GET quests/options` | Optional `quest_id` | Visible live quests, `QuestUtility.CanAcceptQuest`, per-part requirements, legal accepters, indexed reward options. Explicit ID can read a historical/hidden quest. |
| `POST quests/accept` | `quest_id`, `pawn_id`, optional `choices:[{part_index,choice_index}]`, `confirm:true` | Validate all requested indexes and all unresolved choices before `QuestPart_Choice.Choose` and `Quest.Accept(pawn)`. Every multi-option part needs an explicit choice; a single option needs none. |
| `GET dlc/ideology/archonexus` | None | Quest stages, actual carry-dialog rows/limits, tile/ideology stage, existing ideos, core IDs/maps, countdown. |
| `POST dlc/ideology/archonexus/select` | `quest_id`, `thing_ids`, `confirm:true` | Select exact grouped rows from the live native dialog; native `AcceptanceReport` enforces 1–5 people, up to 5 animals, cycle relic limit and 7 item rows. Invoke only its newly produced native confirmation. |
| `POST dlc/ideology/archonexus/tile` | `quest_id`, `tile_id`, `layer_id` (1.6; default 0) | Run the active native tile validator, then its chosen callback; invalid/occupied terrain is rejected. Wait for `choose_tile` after the cinematic. |
| `POST dlc/ideology/archonexus/ideology` | `quest_id`, exactly one of `keep_ideology:true`, `primary_ideo_id`, or `ideo:{mode:fixed|fluid|load,...}`; optional `pawn_ideos:{Human123:ideo_id}`; `confirm:true` | Existing-ideo selection, new fixed/fluid creation, or native saved `.rid` load, colonist assignment, then the native Next-button body and `QuestPart_NewColony.InitMoveColony` continuation. Queues the native long event, reset and completion signal. |
| `POST dlc/ideology/archonexus/cancel` | `quest_id` | Native carry cancellation before confirmation; quest returns to `NotYetAccepted`. The later picker has both Escape and its Cancel button disabled, so its internal teardown callback is not exposed. |
| `POST dlc/ideology/archonexus/study` | `structure_id`, `enabled` | Native `CompStudiable.SetStudyEnabled`; both archotech structures default to disabled. Research priority/schedule and native `WorkGiver_StudyInteract` produce study completion. |
| `POST dlc/ideology/archonexus/activate` | `core_id`, `pawn_id`, `confirm:true` | Exact core's native multi-select menu submits `ActivateArchonexusCore`; the driver reaches the interaction cell and calls `Activate`, starting the six-second ending countdown. |

Use IDs returned by these reads. An item row can represent several compatible stacks;
its `carry_count` is the quantity the native dialog will normalize, subject to
`ThingDef.ArchonexusMaxAllowedCount`. Arbitrary items, duplicate IDs and no-colonist
selections are rejected. Dry selection/acceptance does not alter the live selection or quest.
The first two cycle quests sell/restart; the third accepts the final site quest and does not restart the colony.
The native game still requires wealth, alliance and (in later cycles) structure study.
These endpoints neither generate quests nor bypass those requirements.

Carry confirmation starts the original cinematic. It also immediately normalizes chosen
item quantities/HP, frees an all-slave selection and removes the escape ship, exactly as
native confirmation does. There is no player cancellation after this confirmation; the native tile picker disables Escape and Cancel.
The final ideology confirmation abandons the old colonies, unselected people/items and
resets research/relations through native code. There is no API rollback.
`migration_completed:false` means the request has advanced the workflow only; verify the
quest's `EndedSuccess`, new settlement tile/map, old map removal, and retained people after
native drop pods arrive. Classic mode can queue generation directly from tile choice.

Ideology restart selection supports keeping ideologies, selecting existing ideos and
converting selected restart colonists. ID-R11 also accepts a new fixed/fluid ideology
through the same C-04 `ideo` configuration vocabulary (`mode`, `name`,
`structure_meme`, `memes`, `styles`); it is generated with the native
`IdeoGenerator` and selected through the live `Dialog_ConfigureIdeo`
(`MakeIdeoPrimary`/`customOrLoadedIdeo`), then the exact native Next body runs. The
endpoint requires the live dialog's native `forArchonexusRestart:true`; the ordinary
MainTab ideology dialog is rejected. No new reform/load engine and no ordinary
runtime ideology load are exposed.

Saved `.rid` files use `ideo:{mode:"load",preset_file:"my-preset.rid"}`. Discover
names and native header warnings in `GET game/start/ideo/options.saved_presets`.
Only files in the native profile `Ideos` directory are accepted; paths return400,
missing files404, unreadable headers422. Compatibility warnings require nested
`ideo.confirm_warnings:true` (409 otherwise); `confirm:true` separately authorizes
the native colony reset. Creation fields cannot be mixed with file load.
`dry_run:true` reads only the header, returns `mode:"load_preset"`,
`primary_ideo_id:null`, `loaded:{saved_preset,would_load:true}`, and does not load
or assign an ideology. Confirmed load calls native `TryLoadIdeo`/`InitLoadedIdeo`
and returns the new destination `primary_ideo_id`, `loaded` identity/content
summary, and the ordinary queued migration result. Full loading can still fail
after a readable header. Loaded fluid development starts fresh under native
external-preset rules. All pending pawn conversions and map migration retain
the existing Next path.

Saved-file new-game and restart validation is separately recorded in
[the 2026-11-13 load review](../references/reviews/2026-11-13-ideology-saved-preset-load.md).
Its fixture prepares only the restart choice; natural offer/carry/cinematic
evidence remains the earlier ID-09a runs, not this fixture scenario.

ID-R11 实测（2026-11-13，g35 隔离副本）：fixture 用最小 ongoing
`EndGame_ArchonexusVictory_FirstCycle` + `QuestPart_NewColony.TileChosen` 打开真实
restart 对话框（不生成/接受任务、不走 carry/过场），API 依次验证
create fixed（`Archonexus Reforged`，`Structure_Animist`+`MaleSupremacy`+`Hindu`）、
create fluid（`Archonexus Fluid`，单个 normal meme）与 load existing（含
`pawn_ideos` 显式转换），均 `applied:true` 后由原生 `InitMoveColony` 迁移到
tile4 新家；读回新 primary ideo、选中 pawn 的 ideo、旧 map 移除。dry-run（fixed/
fluid/load）不新增 ideo、不改 dialog/primary。证据
`tools/cua/.runtime/id-r11-run7-20261113/evidence/id-r11.json`（create）、
`id-r11-run5-20261113/evidence/id-r11.json`（load）、
`id-r11-run8-20261113/evidence/id-r11.json`（fluid），runner
`tests/parallel/ideology/id_r11_archonexus_ideo.py`，fixture
`tests/parallel/ideology/IdR11Fixture.cs`。
未测自然叙事器 offer/carry/过场、classic、preset 重开新建、多 restart 对话框、
重开对话框取消、1.5。
The API does not accept unrelated windows or tile pickers. Native callbacks must belong
to the requested live `QuestPart_NewColony`; reload uses the game's own restart-offer reversion.

Core activation returns `job_started`/`job_id` and `ending_completed:false`. Verify the
countdown followed by the native credits screen; `applied:true` is only job submission.
No API calls the core's dev gizmo or `Activate` directly. The tested game version and
fixture preparation are separately recorded in the phase evidence; 1.6 validation does not certify 1.5.

F-ID 来源核对（Ideology 全部行、已验/未测/差集）见
[2026-11-13 F-ID 来源核对包](../references/reviews/2026-11-13-f-ideology-source-audit.md)。
