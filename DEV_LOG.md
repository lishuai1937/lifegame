# LifeGame Development Log
# Last Updated: 2026-04-07

## Project Overview
A life simulator game combining board game (like Monopoly) + open world (like GTA).
Each grid cell = an age (1-100). Player rolls dice to advance through life.
Core loop: Roll dice -> Land on age grid -> Enter small open world -> Complete event -> Return to board.
Death triggers karma judgment -> Heaven/Hell -> Optional reincarnation with soul memories.

## What Has Been Completed

### 1. Architecture & All C# Scripts (DONE)
Location: lifegame/Assets/Scripts/

- Core/GameManager.cs        - Singleton, state machine (MainMenu/BoardGame/OpenWorld/CG/Death/Reincarnation)
- Core/GameFlowController.cs - Runtime controller, wires UI buttons to game logic, handles full game loop
- Core/GameEnums.cs           - All enums (GameState, WorldRealm, AgePhase, DiceSpeed, GridType)
- Core/SaveSystem.cs          - JSON save/load system
- Data/PlayerData.cs          - Player data (age, gold, karma, family, achievements, soul memories)
- Data/GridData.cs            - Grid cell data structure, GridWorldResult
- BoardGame/BoardManager.cs   - Board navigation, dice trigger, grid event dispatch
- BoardGame/DiceSystem.cs     - Dice roll logic (Slow=1-3, Fast=3-6)
- BoardGame/FamilyGenerator.cs- Random family background at age 6 (wealth level, trait, initial gold)
- BoardGame/KarmaJudge.cs     - Death judgment (karma>0=Heaven, <0=Hell), reincarnation with soul memory
- OpenWorld/PlayerController.cs - Third person character controller (WASD+Shift sprint, E interact, ESC exit)
- OpenWorld/IInteractable.cs  - Interaction interface
- OpenWorld/EventTrigger.cs   - In-world event trigger with death risk, karma impact, gold reward
- UI/UIManager.cs             - Panel show/hide management for all UI states
- Audio/AudioManager.cs       - BGM per age phase, SFX playback

### 2. Editor Auto-Build Script (DONE)
Location: lifegame/Assets/Scripts/Editor/LifeGameSceneBuilder.cs

In Unity menu: LifeGame -> Build All
Auto-generates:
- 30 grid cells in a circle (color-coded by age phase)
- Player token (red sphere), Dice visual (white cube, spins on roll)
- Full UI Canvas: MainMenu, BoardGame, DicePanel, SpeedChoice, PlayerInfo HUD, EventDialog, DeathPanel, CGPanel
- Camera (top-down), Directional light, Ground plane
- GameManager + GameFlowController with all references auto-wired
- EventSystem

### 3. Grid Event Data (DONE)
Location: lifegame/Assets/Resources/GridData/grids_sample.json
- 18 key age events from birth to age 100
- Each has: age, type, title, description, gold reward, karma impact, death risk/probability

### 4. Console Demo EXE (DONE)
Location: lifegame/ConsoleDemo/publish/LifeGame.exe
- Standalone text-based demo, full game loop playable
- 30+ events with choices, karma system, family generation, death/heaven/hell/reincarnation
- Self-contained .NET 9 exe, no runtime needed

## What Needs To Be Done Next (On Unity Computer)

### Step 1: Open Project in Unity
- Install Unity Hub + Unity 2022.3 LTS (or newer)
- Open -> select lifegame folder -> wait for import
- Menu: LifeGame -> Build All -> Press Play -> verify it runs

### Step 2: Import Art Assets (Use AI Tools)
Recommended AI tools for asset generation:
- Midjourney / Stable Diffusion + ComfyUI -> UI textures, CG illustrations, backgrounds
- Meshy / Tripo3D -> Text-to-3D models (buildings, props, characters)
- Mixamo (free) -> Character animations
- Suno / AIVA -> BGM for each age phase
- ElevenLabs / Azure TTS -> NPC voice lines

### Step 3: Scene Building
Kiro can help with:
- Writing more Editor scripts to auto-place imported models
- Writing shaders and material configs
- Writing scene layout scripts (procedural building/terrain placement)
- Writing UI animation and transition code
- Any C# code needed

You handle:
- Running AI tools to generate assets
- Dragging generated assets into Unity project
- Final visual tweaking and testing

### Step 4: Content Expansion
- Expand grids_sample.json to cover all 100 ages (Kiro can batch-generate with AI)
- Build 5-6 template open world scenes (city, school, beach, hospital, countryside, home)
- Reuse scenes with different lighting/props/NPCs for different ages
- Add weather system, gender-specific dialogue branches
- Millennium/dreamcore aesthetic: bloom + noise + film grain post-processing, WinXP-style UI

## Key Design Decisions
- Grid cells = ages (1-100), core gameplay is 18-60
- Dice speed choice every 20 grids (slow 1-3, fast 3-6)
- Family background revealed at age 6, determines initial gold
- Karma system: positive=good, negative=evil, determines afterlife
- Reincarnation preserves achievements as soul memories
- Scene reuse strategy: same base scene + parameter changes (lighting, NPCs, props) for different ages
- Art style varies by age phase: low-poly childhood -> anime youth -> semi-realistic adult -> minimalist elder

## Tech Stack
- Engine: Unity 2022 LTS
- Language: C#
- Data: JSON-driven grid events
- Save: JSON via JsonUtility
- Target: PC (Windows exe)
## Update: 2026-04-07 - Switched to 2D Board + 3D World Hybrid

### What Changed
- BoardManager.cs: Rewritten for 2D side-scrolling layout (Mario-style zigzag)
  - Grid cells laid out left-to-right, zigzag up (10 per row)
  - Player token hops between cells with arc animation (coroutine)
  - GetCellPosition() maps age index to 2D world coordinates
  - ResetBoard() method for reincarnation
- NEW: BoardCamera2D.cs - Orthographic camera that follows player token smoothly
- LifeGameSceneBuilder.cs: Completely rewritten for 2D board
  - Generates 100 grid cells as colored quads with age labels
  - Milestone ages (6,12,18,30,50,60,80,100) are larger and highlighted
  - Zigzag connectors between rows
  - Separate 3D OpenWorld area (hidden below board, activated on grid entry)
  - 3D area has ground, buildings, light, player capsule, sample event trigger
- GameFlowController.cs: Updated to work with 2D board token positioning

### Architecture (unchanged)
- Board layer: 2D orthographic, side-scroll, sprites/quads
- Grid world layer: 3D perspective, third-person, open world
- State machine switches between them seamlessly
- All data structures, enums, save system, dice, karma, family - unchanged
## Update: 2026-04-07 - Karma System Redesign (Hidden Karma)

### Design Change
Old: Karma was predefined in grid data (KarmaImpact field), shown to player during choices with [+2] [-1] hints
New: Karma is HIDDEN from player, only driven by actions inside open world

### How It Works Now
1. Board layer: Player sees name, age, gold. NO karma display anywhere
2. Open world: Player makes choices (help someone, steal, ignore, etc.)
   - EventTrigger calls KarmaTracker methods (HelpedSomeone, HarmedSomeone, SelfishChoice, etc.)
   - Player only sees narrative text ("You helped the old man cross the street"), never sees numbers
   - Each action has RANDOMIZED karma value (e.g. Help = +1 to +3 random, Harm = -1 to -3 random)
3. On world exit: KarmaTracker.SettleAndReset() totals all actions, adds to PlayerData.KarmaValue
4. At death: Karma revealed as descriptive text only ("A truly kind soul" / "Some regrets linger"), then Heaven/Hell judgment

### Files Changed
- Data/GridData.cs: Removed KarmaImpact field from GridData
- NEW: OpenWorld/KarmaTracker.cs: Singleton that logs player actions with hidden karma values
  - Methods: HelpedSomeone(), HarmedSomeone(), SelfishChoice(), SelflessChoice(), NeutralAction(), IgnoredSomeone()
  - SettleAndReset() returns total karma and clears log
- OpenWorld/EventTrigger.cs: Rewritten to use KarmaTracker instead of direct karma modification
  - NEW: EventChoice class with KarmaActionType enum (Help/Harm/Selfish/Selfless/Neutral/Ignore)
  - Player sees choice text + narrative result, never karma numbers
- OpenWorld/PlayerController.cs: Exit now calls KarmaTracker.SettleAndReset()
- Core/GameFlowController.cs: 
  - HUD no longer shows karma
  - Death screen shows descriptive text instead of karma number
  - Removed all direct karma manipulation from board events
- Resources/GridData/grids_sample.json: Removed KarmaImpact from all entries
## Update: 2026-04-07 - Content + Dialogue System + Console Demo v2

### 1. 100 Grid Events Complete
File: Assets/Resources/GridData/grids_full.json
- All 100 ages covered (1-100)
- Childhood (1-12): birth, first words, kindergarten, school, secret base
- Youth (13-17): rebellion, middle school, part-time job, high school
- Young (18-30): gaokao, university, graduation, first job, love, career choice
- Prime (31-50): mortgage, wedding, child, midlife crisis, accident, parents aging
- Middle (51-65): hobbies, retirement countdown, grandchild, travel, calligraphy
- Elder (66-100): old photos, golden wedding, wheelchair, memoir, final sunset
- Death probability increases with age (0% childhood -> 50% at 100)

### 2. Dialogue System (NEW)
Files:
- Data/DialogueData.cs: DialogueTree, DialogueNode, DialogueChoice data classes
- OpenWorld/DialogueSystem.cs: Singleton dialogue manager with branching tree support
  - StartDialogue(), PickChoice(), Continue(), EndDialogue()
  - Choices feed into KarmaTracker (hidden karma)
  - Events: OnDialogueStarted, OnNodeChanged, OnDialogueEnded
- OpenWorld/NPC.cs: NPC component, implements IInteractable, loads dialogue from JSON
- UI/DialogueUI.cs: UI controller that listens to DialogueSystem events
- Resources/Dialogues/sample_npc.json: Example NPC dialogue with 3 choices

### 3. Console Demo v2 (Updated + Recompiled)
File: ConsoleDemo/publish/LifeGame.exe
Changes:
- Karma is now HIDDEN - no [+2] [-1] hints on choices
- Choices show only narrative text, player decides based on story not numbers
- Karma calculated with randomized values per action type
- Death screen shows descriptive text ("A truly kind soul") instead of karma number
- HUD shows name/age/gold only, no karma display
- Same 30+ events with choices, all using new hidden karma system
## Update: 2026-04-07 - Shadow People NPC System

### CORE DESIGN RULE (MUST KEEP):
All NPCs in the open world start as BLACK SILHOUETTES ("shadow people").
Only after the player TALKS to them, their true appearance is revealed with a transition effect.
Important/story NPCs can bypass this and show their true form immediately.
This is a fundamental design element of the game, not optional.

### Implementation
- NEW: OpenWorld/NPCReveal.cs
  - Attached alongside NPC component
  - IsImportantNPC: if true, shows true form from start
  - ApplyShadowAppearance(): makes NPC all black on spawn
  - Reveal(): triggers transition coroutine (black -> white -> true material)
  - Supports both material swap AND model swap (ShadowModel/TrueModel)
  - Optional particle effect on reveal
  - RevealDuration configurable (default 0.8s)

- UPDATED: OpenWorld/NPC.cs
  - First Interact() call triggers NPCReveal.Reveal()
  - Before reveal: interaction prompt shows "Press E: ???" (mysterious)
  - After reveal: shows "Press E: Talk to [NpcName]"
  - hasSpoken flag prevents re-triggering reveal

### How to use in Unity:
1. Create NPC GameObject with NPC + NPCReveal components
2. Assign ShadowMaterial (black unlit) and TrueMaterial (real appearance)
3. Optionally: use ShadowModel (black humanoid) and TrueModel (real model) for full model swap
4. Set IsImportantNPC = true for story-critical characters
## Update: 2026-04-07 - Afterlife Gameplay + Procedural Scene Generation

### 1. Afterlife System (Heaven & Hell)
Files:
- Core/AfterlifeManager.cs: Manages afterlife state, camera switching, karma bonus
  - EnterAfterlife(realm): hides board, shows heaven/hell world
  - Player can explore for up to 2 minutes, then reincarnation prompt
  - Actions in afterlife earn karma bonus that carries to next life
  - ESC to leave early
- OpenWorld/AfterlifeEvent.cs: Event triggers specific to afterlife
  - Heaven events: peaceful, sharing stories, gaining wisdom (+karma)
  - Hell events: trials, offering comfort to tormented souls (+karma for redemption)
  - Exit points trigger reincarnation

Heaven world: white/gold ground, cloud platforms, golden gate, light pillar, angel NPCs
Hell world: dark red ground, lava pools, fire pillars, broken gate, tormented soul NPCs

### 2. Procedural Scene Generator
File: OpenWorld/SceneGenerator.cs
Generates complete scenes from code based on SceneId and age:

Scene templates (all using primitive shapes as placeholders):
- Home (home_baby/child/teen/parents/old_home): house, furniture, yard, tree
- School (kindergarten/primary/middle/high/university): building, playground, flagpole, gate
- City (office/startup/apartment/suburb/park): road, buildings with windows, street lamps
- Hospital: white building, red cross, ambulance, bench
- Countryside: fields, trees, river, cottage
- Beach (beach_town/travel): ocean, palm trees, umbrella
- Town (restaurant/tea_house/market/shop): shops, road, signs
- Wedding Hall: hall, arch, red carpet
- Park: pond, benches, trees
- Dream: surreal floating objects, childhood river (purple ground)
- Heaven: clouds, golden gate, light pillar, angel NPCs
- Hell: lava pools, fire pillars, broken gate, tormented NPCs

Features:
- Lighting changes by age phase (warm childhood -> harsh prime -> dim twilight elder)
- NPCs auto-spawned with shadow system (black silhouettes, reveal on talk)
- Important NPCs (teachers, family, story characters) show true form immediately
- Exit points auto-placed in each scene
- Afterlife NPCs always visible (no shadow)
## Update: 2026-04-07 - NPC Dialogue JSONs (26 files)

### Dialogue files created in Assets/Resources/Dialogues/
Each file is a complete dialogue tree with branching choices that feed into hidden karma.

Home scene:
- home_mom.json: Mom asks about kindergarten (help/neutral/selfish choices)
- home_dad.json: Dad teaches bike riding
- home_neighbor.json: Neighbor asks help finding lost cat
- home_spouse_old.json: Elderly spouse suggests a walk

School scene:
- school_teacher.json: Teacher notices grades dropping
- school_classmate.json: Classmate wants to copy homework
- school_shopkeeper.json: Snack shop outside school gate

University:
- university_roommate.json: Roommate invites to hotpot

City/Work scene:
- city_boss.json: Boss asks to work overtime
- city_colleague.json: Colleague worried about layoffs
- city_homeless.json: Homeless person asks for food

Hospital:
- hospital_doctor.json: Doctor delivers health check results
- hospital_nurse.json: Nurse gives directions
- hospital_patient.json: Fellow patient makes conversation

Countryside:
- countryside_farmer.json: Old farmer offers to teach farming

Beach/Travel:
- beach_traveler.json: Solo traveler suggests exploring together

Town:
- town_shopkeeper.json: Fruit shop owner

Park:
- park_elder.json: Old man wants a chess partner
- park_child.json: Child needs help getting kite from tree

Wedding:
- wedding_partner.json: Partner on wedding day

Dream:
- dream_memory.json: Childhood self appears in dream

Afterlife:
- heaven_angel.json: Guardian angel reflects on your life
- heaven_spirit.json: Peaceful spirit shares their story
- hell_soul.json: Tormented soul begs for help
- hell_gatekeeper.json: Gatekeeper questions your self-awareness

All dialogues use KarmaActionType (0=Neutral,1=Help,2=Harm,3=Selfish,4=Selfless,5=Ignore)
Player never sees karma values, only narrative text and choices.
## Update: 2026-04-07 - Expanded NPC Dialogues (52 total)

Added 26 more NPC dialogue files. Full list now:

Family: mom, dad, grandpa, grandma, neighbor, childhood_friend, spouse_old, child_yours, grandchild
School: teacher, classmate, shopkeeper, principal, dean, crush, guard, kindergarten_teacher
University: roommate, professor
City/Work: boss, colleague, homeless, courier, landlord, blind_date, taxi_driver, gym_trainer, interviewer, delivery_guy, lawyer, therapist, stray_cat
Town/Market: town_shopkeeper, market_vendor, driving_instructor
Park: elder, child, dancing_auntie
Hospital: doctor, nurse, patient
Countryside: farmer
Beach: traveler
Wedding: partner
Community: calligraphy teacher
Old age: caretaker
Dream: memory (childhood self)
Afterlife: heaven_angel, heaven_spirit, hell_soul, hell_gatekeeper
Sample: sample_npc

All 52 files in Assets/Resources/Dialogues/
Each has 2-3 branching choices with hidden karma via KarmaActionType.
## Update: 2026-04-08 - Major Design Decisions (Player Feedback)

### 1. BRANCHING PATH SYSTEM (replaces linear board)
- Board is NO LONGER a straight line. After key ages, the path BRANCHES based on player performance
- Example: Age 18 (Gaokao) - performance in the exam grid world determines which branch you take:
  - Good exam -> "University path" (different grid events)
  - Bad exam -> "Work path" (different grid events)
- Money/wealth affects branches too: enough money -> business path -> boss or failure
- Each branch has DIFFERENT grid events, NPCs, and storylines
- Branches can merge back at certain ages (e.g. everyone hits "midlife crisis" at 35-40)
- Previous grid world performance accumulates and influences future branches

### 2. NPC RELATIONSHIP SYSTEM (persistent NPCs across grid worlds)
- NPCs have an AFFINITY/INTIMACY system
- Interacting positively with an NPC increases affinity
- When affinity reaches a threshold, that NPC FOLLOWS you to the next grid world
- Recurring NPCs are NOT shadow people in subsequent worlds (already revealed)
- NPCs can be life-changing mentors, friends, lovers, rivals
- NPCs also age, get sick, and can die in later grid worlds
- Some NPCs can fundamentally change your life path (e.g. a mentor opens a new branch)
- There is NO absolute right or wrong - just consequences

### 3. GRID WORLD MAIN QUESTS + FORCED EXIT
- Each grid world has a MAIN QUEST (not just dialogue)
- Player is FORCED OUT of grid world when certain conditions are met (time limit, quest complete, or triggered event)
- Hidden quests exist: completing them rewards special items

### 4. SPECIAL ITEMS: Regret Pill + Time Rewind
- REGRET PILL (Hou Hui Yao): Obtained from hidden quests in grid worlds
  - Used on the board (dice phase)
  - Sends player BACK to a specified grid, re-enters that grid world
  - After exiting, returns to the grid BEFORE where you used the pill
  - CANNOT re-enter the current grid world after using pill
- TIME REWIND (Shi Jian Hui Su): Also from hidden quests
  - Extends time in current grid world (delays forced exit)
  - Allows more exploration, more NPC interactions, more hidden content

### 5. ASSET/PROPERTY SYSTEM
- Player can BUY assets: houses, cars, etc.
- Assets affect a new stat: CHARM/CHARISMA
- Charm influences NPC interactions, dialogue options, and event outcomes
- Grid worlds have shops/markets where assets can be purchased
- Assets persist across grid worlds (you keep your car/house)

### 6. PRIORITY: Focus on first life (no reincarnation improvements for now)

### These are CORE DESIGN RULES. All must be implemented.