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
## Update: 2026-04-07 - Social System

### CORE DESIGN:
- Each grid world has ~100 NPCs, but player has LIMITED social energy
- Energy decreases with age (kids=15, young=10, prime=8, elder=4)
- Forces player to choose WHO to invest time in, like real life
- Relationships persist across grid worlds (recurring NPCs)
- On reincarnation, deep bonds become soul memories

### NPC Personality (NPCProfile)
File: Data/NPCProfile.cs
6 personality traits (0-10 scale):
- Kindness: affects willingness to help/forgive
- Ambition: career-driven, competitive
- Humor: lighthearted, jokes
- Loyalty: sticks through hard times
- Temper: easily angered
- Introversion: 0=extrovert 10=introvert

10 NPC roles: Family, Classmate, Colleague, Neighbor, Stranger, Authority, Romantic, Rival, Elder, Child

### Relationship System
File: Data/Relationship.cs
- Closeness: -100 (enemy) to +100 (soulmate)
- 7 stages: Enemy -> Dislike -> Stranger -> Acquaintance -> Friend -> CloseFriend -> Soulmate
- 13 relationship types: Friend, BestFriend, Lover, Spouse, Rival, Mentor, etc.
- 8 social actions: Chat, Help, Gift, Joke, Argue, Betray, Confess, Ignore
- Closeness change depends on NPC personality:
  - Introverts warm up slower to Chat
  - Kind NPCs appreciate Help more
  - Ambitious NPCs care less about Gifts
  - Humorous NPCs love Jokes, serious ones don't
  - High-temper NPCs escalate Arguments
  - Loyal NPCs forgive easier
  - Confessing too early backfires

### Social System Manager
File: OpenWorld/SocialSystem.cs
- Tracks all relationships across the entire game
- Energy system: limited interactions per grid world
- InteractWith() -> closeness change + karma tracking
- GetLifeSummary() for death screen ("A life rich in deep connections")
- GetSoulMemories() for reincarnation (deep bonds carry over)

### NPC Updated
File: OpenWorld/NPC.cs
- Now has NPCProfile with auto-generated personality
- Registers with SocialSystem on spawn
- Interaction prompt shows relationship stage after reveal
- Energy check before allowing interaction
- Dialogue choices map to social actions
## Update: 2026-04-07 - NPC Life Timeline & Event Invitation System

### CORE DESIGN:
- NPCs are not static. They have their own life trajectory that runs parallel to the player's.
- NPCs age, get married, have children, face crises, retire, and die.
- Close NPCs (Friend+) INVITE the player to their life events.
- Player can attend (boosts relationship, costs energy) or skip (hurts relationship).
- When you attend an NPC's wedding, you meet their spouse (new NPC).
- When they have kids, you can meet the children (new NPCs with inherited traits).
- Childhood NPCs have their own parents with different personalities.

### NPC Lifeline System
File: Data/NPCLifeline.cs
Each NPC gets a full life timeline on creation:
- Birth year (relative to player, +/- 5 years)
- Death age (60-100, kind NPCs live longer)
- Family tree: father, mother, spouse, children (all are NPCProfile IDs)
- Occupation based on personality (ambitious=CEO/Lawyer, kind=Teacher/Nurse, normal=Clerk/Cook)
- Life events auto-generated:
  - Graduation (18, 22 if ambitious)
  - First job
  - Marriage (if not too introverted, age 24-33)
  - Children (1-2 kids after marriage)
  - Birthday milestones (30, 40, 50, 60, 70, 80)
  - Random crisis (50% chance): financial trouble, health crisis, or life dilemma
  - Retirement (55-65)
  - Death

### Event Invitation System
File: OpenWorld/NPCEventManager.cs
- CheckEventsAtAge(): called when player advances age, scans all tracked NPCs
- Only NPCs with closeness >= 20 (Friend) send invitations
- Sorted by closeness (closest friends first)
- Player responses:
  - AttendEvent(): boosts relationship, narrative text, meets family members
  - HelpWithCrisis(): costs gold, big relationship boost, karma bonus
  - SkipEvent(): relationship takes a hit, especially for weddings/funerals
- GetOrCreateFamilyMember(): generates spouse/child NPCs when player meets them
- NPC death is tracked, dead NPCs stop generating events
## Update: 2026-04-07 - Dream/Career System

### DESIGN:
Player is asked "What is your dream?" at 4 milestone ages:
- Age 6: First dream (childhood version, e.g. "I want to be a superhero!")
- Age 18: Before gaokao, CG cutscene asks again (can keep or change)
- Age 22: After university graduation, ask again
- Age 30: Final ask at "standing on your own" milestone

If dream stays the SAME across all 4 asks: "Unwavering Heart" achievement
- Karma bonus, gold multiplier doubled, unique CG, soul memory on reincarnation
If dream changes: "Adaptability" bonus (social energy +1 per change)

### 22 Career Options:
Each has unique gameplay bonuses:

| Career | Bonus |
|--------|-------|
| Astronaut | Death prob -15%, unlock space dream scene |
| Doctor | Death prob -20%, can heal NPC crises, +karma |
| Teacher | Social energy +3, NPC children gain better traits |
| Artist | Richer dream scenes, special CG at milestones |
| Athlete | Gold +80% before 40, death prob +10% after 60 |
| Scientist | Hidden events revealed in each grid world |
| Businessman | Gold +60%, social energy -2 |
| Chef | Gold +30%, Gift action closeness doubled |
| Musician | Chat closeness +50%, special BGM, music events |
| Writer | Memoir karma +5, expanded dialogue, +2 soul memories |
| Programmer | Gold +50%, social energy -1, hidden tech events |
| Lawyer | Can defend NPCs in crises, gold +40% |
| Soldier | Death prob +5%, karma +2 per world, NPC respect |
| Farmer | Countryside enhanced, death prob -10%, steady gold |
| Pilot | Travel events double gold, sky scenes, death +3% |
| Detective | Discover NPC secrets, hidden events, +karma |
| Firefighter | Can save NPCs from death, karma +3, death +5% |
| Vet | Kind NPCs gain closeness faster, animal companion |
| Architect | City scenes enhanced, gold +35%, building events |
| Journalist | Interview NPCs for secrets, +karma for justice |
| Superhero | Childhood-only. If kept to 30: legendary, karma x2 |
| Undecided | No bonus/penalty, maximum freedom |

### Files:
- Data/DreamSystem.cs: Full dream tracking, career bonuses, modifiers
- Data/PlayerData.cs: Updated with DreamSystem field

### Integration points (for Unity):
- BoardManager: At ages 6/18/22/30, trigger dream selection UI
- GameFlowController: Apply gold multiplier, death prob modifier, energy modifier
- SceneGenerator: Career-specific scene enhancements
- SocialSystem: Apply energy modifier from career
## Update: 2026-04-07 - Career Bonuses Integrated Into All Systems

### What was connected:

BoardManager.cs:
- Dream milestone triggers at ages 6, 18, 22, 30 (TODO: UI selection)
- Career death probability modifier applied to grid events
- Career gold multiplier applied to grid rewards
- Athlete: gold bonus only before 40, death penalty only after 60
- Unwavering Heart: gold doubled after 30
- Scientist: 30% chance to discover hidden bonus (+50 gold) per grid
- Soldier: +2 karma per grid world entered
- NPC event invitations checked at each age

SocialSystem.cs:
- Career energy modifier applied on world entry
- Teacher: +3 energy, Musician: +1, Businessman: -2, Programmer: -1
- Musician: Chat closeness +50% bonus
- Chef: Gift closeness doubled
- Vet: Kind NPCs gain closeness faster

EventTrigger.cs:
- Doctor: can heal NPC health crises (nullifies death, +karma, +gold)
- Firefighter: can save NPCs from death events (nullifies death, +3 karma)
- Lawyer: can defend NPCs in injustice events (+gold, +karma)
- Detective: discovers NPC secrets (hidden text + gold)
- Journalist: discovers secrets + exposes injustice (+karma)
- Proximity hints show career abilities ("[Doctor: Can Heal]")

GameFlowController.cs (Death screen):
- Writer: memoir at 85+ gives +5 karma
- Unwavering Heart: +10 karma bonus at death
- Career name shown on death screen
- Social life summary shown on death screen
- Writer: +2 extra soul memories on reincarnation
- Social soul memories (deep bonds + unresolved conflicts) carry to next life
- SocialSystem and NPCEventManager reset on reincarnation
## Update: 2026-04-07 - Phone/Contact System

### DESIGN:
- Phone unlocked at age 12 (basic: call only)
- Smartphone upgrade at age 18 (adds texting)
- Contact list = every NPC you've talked to (auto-added on first dialogue)
- Can use phone from board layer OR open world
- Contacts sorted by closeness (closest friends first)
- Cooldown prevents spamming same NPC

### Phone Actions:
| Action | Energy Cost | Requirements | Effect |
|--------|------------|--------------|--------|
| Call | 1 | Age 12+ | Closeness +1 to +3 (introverts less) |
| Text | 0 (free) | Age 18+ | Closeness +1 if close, 0 if not |
| Borrow Money | 0 | Friend+ (closeness 20+) | Get gold, closeness -2 (debt tension) |
| Lend Money | 0 | Have enough gold | Closeness +3 to +8 based on amount, +karma |
| Invite to Event | 0 | Any contact | Accept rate based on closeness + personality |

### NPC Response Logic:
- Closeness 50+: picks up immediately, enthusiastic
- Closeness 20-50: answers, friendly
- Closeness 0-20: answers after rings, neutral
- Closeness <0: doesn't pick up
- Introverts: less closeness gain from calls, less likely to attend events
- Kind NPCs: more likely to lend money
- Borrow limits: need Friend+ level, high amounts need CloseFriend+

### Files:
- NEW: OpenWorld/PhoneSystem.cs - Full phone system with all actions
- UPDATED: OpenWorld/NPC.cs - First dialogue auto-adds NPC to phone contacts

### Integration:
- PhoneSystem.CheckUnlock(age) should be called when player advances age
- Phone UI (TODO in Unity): contact list, action buttons, chat history
## Update: 2026-04-07 - World Events + NPC Manipulation + Player Stats

### DESIGN PHILOSOPHY:
"Life is not controllable." The player should feel regret, attachment, helplessness, and occasional joy.
NPCs provide emotional value AND emotional damage. The world pushes back.

### Player Hidden Stats (PlayerStats.cs)
5 hidden attributes (player never sees numbers):
- Charisma (0-20): influence NPCs, persuade, lead
- Resilience (0-20): endure hardship, survive trauma
- Willpower (0-20): resist manipulation, break free from toxic people
- Empathy (0-20): sense NPC emotions, detect manipulation early
- Luck (0-20): bias random events toward positive/negative

### World Event System (WorldEventSystem.cs)
Random uncontrollable events at each age, based on life phase:

School (7-17):
- Bullying: high Resilience+Willpower = stand your ground (+achievement), low = suffer but gain Resilience
- Toxic friend: high Empathy = see through them, low = get used
- Teacher who believes in you (positive, +Charisma)
- First heartbreak (+Resilience)

Young Adult (18-30):
- Manipulative partner: high Willpower = break free (+achievement), low = lose gold + Willpower
- Sudden layoff (-gold, +Resilience)
- Stranger's kindness (positive, +Empathy)
- Betrayal by close friend (+Resilience)
- Lucky windfall (positive, +gold)

Midlife (31-50):
- Loss of loved one (+Resilience, +Empathy, emotional gut-punch)
- Office politics: high Charisma = navigate it, low = get screwed
- Divorce/separation (-gold, +Resilience)
- Child's achievement (positive, +Empathy)

Late Career (51-65):
- Health scare (+Resilience)
- Old friend reconnection (positive, +Empathy)

Elder (66+):
- Outliving friends (emotional)
- Grandchild's love (positive, +Empathy)

Each event has an "EmotionalImpact" line - a gut-punch sentence for maximum feels.

### NPC Influence System (NPCInfluenceSystem.cs)
Two-way manipulation:

NPC -> Player:
- Manipulative NPCs (high ambition + low kindness) try to control player
- Player's Willpower + Empathy determines resistance
- Resisting = +Willpower, +achievement "Stood Your Ground"
- Failing = lose gold, feel uneasy
- Passive influence: very close manipulative NPCs silently steer decisions (high Empathy detects it)

Player -> NPC:
- High Charisma lets player persuade/influence NPCs
- Success = NPC complies, but costs karma (selfish action)
- Failure = closeness drops, NPC resents being pushed
- Creates moral dilemma: you CAN control people, but should you?

### Files:
- NEW: Data/PlayerStats.cs - 5 hidden stats
- NEW: OpenWorld/WorldEventSystem.cs - Random life events by age phase
- NEW: OpenWorld/NPCInfluenceSystem.cs - Manipulation in both directions
- UPDATED: Data/PlayerData.cs - Added PlayerStats field
## Update: 2026-04-07 - NPC Bulk Spawner + Dialogue Auto-Generator

### Problem Solved:
- Before: 2-3 NPCs per scene, 26 hand-written dialogue JSONs
- After: 100 NPCs per scene, every NPC has unique auto-generated dialogue

### NPC Spawner (NPCSpawner.cs)
- Generates 100 NPCProfiles per grid world
- 100 male names + 100 female names + 30 surnames = thousands of combinations
- Role distribution varies by scene:
  - School: 60% classmates, 10% teachers, 10% strangers, 10% rivals, 5% romantic, 5% family
  - City/Office: 40% colleagues, 15% strangers, 10% bosses, 10% rivals, 10% romantic, 10% neighbors
  - Hospital: 30% patients, 25% doctors/nurses, 20% family, 15% neighbors, 10% elders
  - Countryside: 30% neighbors, 20% elders, 20% family, 15% strangers, 10% children
  - Home: 30% family, 30% neighbors, 15% strangers, 10% children, 10% elders
- NPC ages vary around player age (+/- 10 years)
- Seeded random ensures same NPCs regenerate consistently

### Dialogue Generator (DialogueGenerator.cs)
Auto-generates dialogue trees from NPC personality:
- Greeting pools by role (10+ greetings per role)
- Personality modifiers add extra lines (funny NPCs joke, angry NPCs snap, shy NPCs stutter)
- Every dialogue has 3 choices: kind + selfish + contextual
- Contextual choice varies: romantic for love interests, competitive for rivals, wisdom for elders
- NPC responses vary by personality:
  - Kind NPCs: grateful, warm responses to help
  - High-temper NPCs: hostile responses to rejection
  - Ambitious NPCs: respect transactional choices
  - Introverted NPCs: short, awkward responses
  - Humorous NPCs: lighthearted responses

### Integration:
- SceneGenerator now calls SpawnBulkNPCs() for every scene (100 NPCs scattered)
- NPC.cs: if no hand-written JSON, auto-generates dialogue from DialogueGenerator
- Hand-written JSONs (26 files) still take priority for key NPCs
- Important NPCs (Authority, Family) show true form immediately (no shadow)
## Update: 2026-04-07 - Social System v2 (Reworked Energy Model)

### OLD MODEL:
- 1 energy = 1 dialogue interaction
- Energy used up = can't talk to anyone

### NEW MODEL:
- Talking to ANY NPC is FREE (no energy cost)
- Chatting with shadow NPCs: they stay as shadow, small closeness gain
- ENERGY is spent to UNLOCK an NPC:
  - Reveals true form (shadow -> real appearance)
  - Adds to phone contacts
  - Marks as recurring (can appear in future grid worlds)
  - Press F to unlock after first conversation
- Some NPCs auto-unlock for FREE:
  - Family members (always)
  - Romantic interests who are extroverted
  - Very kind + extroverted NPCs (approach you on their own)
  - High player Charisma attracts more NPCs (30% chance for extroverts)

### Unlock Energy per grid world:
- Childhood: 8
- Youth: 6
- Young: 5
- Prime: 4
- Middle: 3
- Elder: 2
- + Career modifier

### Recurring NPCs:
- Unlocked NPCs can reappear in future grid worlds
- Close friends (closeness 30+): always reappear
- Acquaintances (closeness 10+): 40% chance to reappear
- Lovers/spouses: always reappear
- Creates persistent relationships across the entire life

### Player Flow:
1. Enter grid world, see 100 shadow NPCs
2. Talk to anyone for free (they stay shadow, small closeness)
3. Some NPCs walk up to you and reveal themselves (free)
4. Choose which NPCs to spend energy on (F key = unlock)
5. Unlocked NPCs: full relationship, phone contact, reappear later
6. Leave grid world, unlocked NPCs persist

### Files Changed:
- OpenWorld/SocialSystem.cs: Complete rewrite with new energy model
- OpenWorld/NPC.cs: Complete rewrite with free chat + F to unlock + auto-approach
## Update: 2026-04-07 - NPC AI Behavior System (Daily Routine + Approach)

### DESIGN:
NPCs are not static. They live their own life in the grid world:
- Each NPC has a 3-point daily routine (like real people's "3 points 1 line")
- They walk between waypoints, wait at each, then move on
- 15% chance to deviate from routine (wander to a random nearby spot)
- Different roles have different routines:
  - Student: home -> school -> playground
  - Worker: home -> office -> restaurant
  - Elder: home -> park -> market (short distances, slow)
  - Child: home -> playground -> friend's area (wide range, energetic)
  - Family: stays close to home

### Player Approach Logic:
- NPCs that want to approach player have an awareness radius (~6m)
- When player enters that radius, NPC walks toward player
- At ~2m distance, NPC stops, faces player, triggers dialogue automatically
- After dialogue, NPC resumes their routine
- Only approaches ONCE per grid world visit (no stalking)
- If player walks away before NPC reaches them, NPC gives up

### Who Approaches:
- Family members (always)
- Romantic interests who are extroverted
- Very kind + extroverted NPCs
- More NPCs approach if player has high Charisma

### Files:
- NEW: OpenWorld/NPCBehavior.cs - Full AI with routine, deviation, approach states
- UPDATED: OpenWorld/SceneGenerator.cs - NPCBehavior auto-attached to all spawned NPCs
## Update: 2026-04-07 - Approach NPCs: Unique Dialogue + Higher Closeness

### Changes:
NPCs who approach the player now have:

1. Different dialogue (warmer, more initiative):
   - Romantic: "I've been wanting to talk to you for a while..."
   - Family: "There you are! I've been looking for you."
   - Rival: "We need to talk. Now."
   - Friendly stranger: "You look like someone I'd get along with."
   - Funny NPCs: "I promise I'm not weird. Okay, maybe a little. Hi!"
   - Kind NPCs: "You looked like you could use a friend."

2. Player response options match the tone:
   - Warm: "Nice to meet you too! I'm glad you came over." (closeness boost)
   - Cool: "Oh, hi. I'm kind of busy." (NPC backs off)
   - Role-specific: flirty for romantic, confrontational for rival, friendly invite for others

3. Higher initial closeness on approach:
   - Base: +10 closeness (they already like you)
   - Romantic interest: +20
   - Family: +30
   - Kind NPCs: extra +5
   - This means approached NPCs start as Acquaintance or Friend level immediately

### Files:
- DialogueGenerator.cs: Added GenerateApproach() with approach-specific greeting/choice pools
- NPC.cs: AutoUnlock() now sets closeness bonus + loads approach dialogue

### Flow:
NPC approaches -> approach dialogue (warm) -> auto-unlock (free) -> closeness boosted -> added to contacts -> can reappear in future worlds
## Update: 2026-04-07 - Stat Growth System (All 5 Stats)

### File: Core/StatGrowth.cs

### CHARISMA growth:
- Unlock 5 NPCs -> +1
- 3 NPCs approach you -> +1
- NPC accepts your event invite -> +1
- Love confession succeeds -> +2
- Career passive: Businessman/Musician/Lawyer/Journalist +1 per grid world

### RESILIENCE growth:
- Survive any hardship event -> +1
- Survive death risk grid -> +1
- Get betrayed -> +1
- Lose 200+ gold at once -> +1
- Career passive: Soldier/Firefighter/Farmer/Athlete +1 per grid world

### WILLPOWER growth:
- Resist NPC manipulation -> +1
- Break free from toxic relationship -> +2
- Keep same dream at milestone -> +1
- Refuse bribe/temptation -> +1
- Career passive: Soldier/Detective +1 per grid world

### EMPATHY growth:
- Help NPC in crisis -> +1
- Attend NPC funeral -> +1
- Lend money to NPC -> +1
- Have 3+ close relationships -> +1
- Career passive: Doctor/Teacher/Vet/Writer +1 per grid world

### LUCK growth:
- Karma echo: 20% chance of +1 when karma > 10 (good deeds rewarded by fate)
- Find hidden event (Scientist/Detective) -> +1
- Survive 30%+ death probability -> +1
- Reincarnation with soul memories -> +1
- No career passive (luck is fate, not skill)

### Career Passive Stats (applied once per grid world):
| Career | Stat |
|--------|------|
| Businessman, Musician, Lawyer, Journalist | Charisma +1 |
| Soldier | Resilience +1, Willpower +1 |
| Firefighter, Farmer, Athlete | Resilience +1 |
| Detective | Willpower +1 |
| Doctor, Teacher, Vet, Writer | Empathy +1 |
## Update: 2026-04-07 - Asset/Property System

### File: Data/AssetSystem.cs

### Asset Types:

Housing (6 levels):
| Level | Name | Price | Monthly Cost |
|-------|------|-------|-------------|
| None | Homeless | 0 | 0 |
| Rental | Rental Apartment | 100 | -10 |
| SmallApartment | Small Apartment | 500 | -5 |
| House | House | 2000 | -8 |
| BigHouse | Big House | 5000 | -15 |
| Villa | Villa | 15000 | -30 |

Vehicle (6 levels):
| Level | Name | Price |
|-------|------|-------|
| None | Walking | 0 |
| Bicycle | Bicycle | 20 |
| UsedCar | Used Car | 200 |
| NiceCar | Nice Car | 800 |
| LuxuryCar | Luxury Car | 3000 |
| Supercar | Supercar | 10000 |

Business: Start with investment, 5% monthly return, 5% chance of failure per year
Investment: Savings (3% steady), Stocks (-15% to +25%), Real Estate (-2% to +13%), Crypto (-40% to +60%)
Collectibles: Painting, Antique, Jewelry, Rare Book, Wine - appreciate over time (5-12% per year)

### Gameplay Effects:
- Housing >= SmallApartment required for marriage (some NPCs)
- Vehicle multiplies travel event gold (bicycle 1.1x to supercar 2.5x)
- Total assets > 5000 or luxury items attract gold-digger NPCs
- Housing level affects NPC willingness to visit you
- Investments affected by Luck stat
- Economic crisis event: investments -50%, 30% businesses fail
- Bankruptcy: lose everything (+Resilience)
- Death: total assets become inheritance for NPC children
- Collectibles appreciate with age (wine +12%/year!)

### Integration:
- PlayerData.cs: Added AssetSystem field
- ProcessPassiveIncome() called each grid world for business/investment returns
- Luck stat influences investment outcomes
## Update: 2026-04-07 - 7 New Systems Added

### 1. Health System (Data/HealthSystem.cs)
- Physical Health (0-100): decays with age (40+ slow, 60+ faster, 80+ rapid)
- Mental Health (0-100): affected by events, weather, isolation
- Conditions: chronic illness, injury, depression, addiction
- Low health increases death probability (+3% to +15%)
- Depression reduces social energy (-2)
- Good health gives energy bonus (+1)

### 2. Education & Skill System (Data/SkillSystem.cs)
- 6 education levels: None -> HighSchool -> College -> University -> Masters -> PhD
- Skills learned through school and life (6 categories: Academic, Social, Technical, Creative, Physical, Life)
- Education multiplies income (None=0.6x, PhD=2.0x)
- Graduating auto-learns skills (university gives Critical Thinking, Research, Presentation)
- Skills affect event options and NPC interactions

### 3. Reputation System (Data/ReputationSystem.cs)
- -100 (infamous) to +100 (legendary)
- Affects NPC initial closeness (-10 to +10 bonus)
- Player sees vague description ("Well-respected" / "People whisper behind your back")
- Sources: help community +3, public achievement +5, scandal -10, charity +2/+5, betrayal exposed -8

### 4. Diary System (Data/DiarySystem.cs)
- Auto-records all life events (milestones, relationships, hardships, joys, losses, dreams)
- Player can review diary at any time
- Old age: reviewing triggers nostalgia
- Death: highlights become life summary
- Reincarnation: relationship/loss entries become soul memories
- 7 categories: Milestone, Relationship, Achievement, Hardship, Joy, Loss, Dream

### 5. Weather & Season System (OpenWorld/WeatherSystem.cs)
- 9 weather types: Sunny, Cloudy, Rainy, Stormy, Snowy, Cold, Hot, Foggy, Windy
- 4 seasons cycling with age
- Weather affects: mental health (+2 sunny to -3 stormy), NPC activity (30%-100%), event probability
- Luck stat influences weather (high luck avoids storms)
- Bad weather = fewer NPCs outside, more accidents

### 6. Era/News System (OpenWorld/EraSystem.cs)
- World events happen around you based on life decade:
  - Youth: Tech Revolution, Education Reform, Youth Movement
  - Young Adult: Housing Crisis, Startup Boom, Pandemic
  - Midlife: Financial Crisis, Golden Age, War Nearby
  - Late: AI Revolution, Climate Crisis, Medical Breakthrough
  - Elder: Peaceful Era, New Generation
- Era affects economy (-30% crisis to +30% boom)
- News history tracked for life summary

### 7. Last Wishes System (Data/LastWishSystem.cs)
- Unlocks at age 70
- Player picks 3 wishes from 12 templates:
  - "See the ocean one more time"
  - "Reconcile with an old enemy"
  - "Tell someone you love them"
  - "Watch one more sunrise"
  - "Forgive yourself"
  - etc.
- Each wish has emotional completion text
- Completing all wishes: "No regrets" on death screen
- Uncompleted wishes: "So much left undone" - maximum regret

### PlayerData.cs updated with all 7 new fields:
Health, Skills, Reputation, Diary, LastWishes (+ Weather and Era are singletons)
## Update: 2026-04-07 - Full System Integration (All Systems Connected)

### BoardManager v3 - Main Game Loop Integration
Every age step now triggers ALL systems in order:
1. Health decay (physical deteriorates with age)
2. Weather generation (affects mood, NPC activity, events)
3. Era event check (economic crisis hits assets, tech boom boosts income)
4. Asset passive income (business earnings, investment fluctuation, housing costs)
5. Career passive stat growth (doctor +empathy, soldier +resilience+willpower, etc.)
6. Karma echo (good karma occasionally boosts luck)
7. Dream milestone check at 6/18/22/30 (triggers selection UI)
8. Education milestones (auto-graduate at 15/18/22)
9. Phone unlock check (12=phone, 18=smartphone)
10. Last wish unlock at 70
11. NPC event invitations (weddings, funerals, crises from close NPCs)
12. World random events (bullying, layoff, betrayal, windfall - with diary recording)
13. Speed choice every 20 grids
14. Family generation at 6
15. Grid event with ALL modifiers applied:
    - Death probability: career + health + weather combined
    - Gold reward: career multiplier x education multiplier x unwavering heart x vehicle travel bonus
    - Social system initialized with health energy modifier

### GameFlowController v3 - Death & Reincarnation Integration
Death screen now shows complete life summary:
- Career + unwavering heart status
- Education level
- Social life summary (from SocialSystem)
- Health status (chronic illness, final health)
- Reputation description
- Asset/wealth summary
- Last wishes completion status
- Era highlights (what historical events you lived through)
- World event count
- Top 3 diary highlights

Reincarnation now collects ALL soul memories:
- Writer bonus memories
- Social relationship memories
- Diary key entries
- Completed/uncompleted last wishes
- All systems properly reset (Social, NPCEvents, Phone, WorldEvents, Era, StatGrowth)
- Luck +1 from reincarnation

### Editor Script Updated
LifeGameSceneBuilder now creates SystemManagers object with ALL 13 singleton managers:
SocialSystem, PhoneSystem, DialogueSystem, NPCEventManager, NPCInfluenceSystem,
NPCSpawner, WorldEventSystem, WeatherSystem, EraSystem, AfterlifeManager,
SceneGenerator, KarmaTracker, StatGrowth
## Update: 2026-04-07 - Housing System (Floor Plans + Furniture Slots)

### File: Data/HousingSystem.cs

### How it works:
1. Buy a housing level (Rental -> Villa)
2. Pick a floor plan (each level has 1-2 options)
3. Buy furniture to fill slots in each room
4. Furniture gives health/mental/stat bonuses
5. NPC visitors comment on your home, better home = more closeness

### Floor Plans:
| Level | Plan | Slots |
|-------|------|-------|
| Rental | Studio | 2 (bed area, living corner) |
| SmallApartment | 1-Bedroom | 4 (bedroom, living, kitchen, bathroom) |
| SmallApartment | Open Plan | 4 (main space, sleep nook, kitchenette, balcony) |
| House | Family Home | 6 (master bed, kid's room, living, kitchen, bathroom, garden) |
| House | Modern House | 6 (bedroom, guest room, open living, kitchen+dining, study, garage) |
| BigHouse | Luxury Home | 8 (master suite, 2 bedrooms, grand living, kitchen, office, bathroom, backyard) |
| Villa | Estate | 10 (master+2 guest suites, grand hall, entertainment, chef kitchen, library, spa, pool, wine cellar) |

### Furniture Catalog (21 items):
Bedroom: Basic Bed (20g), Comfy Bed (80g, +3 health), Luxury Bed (300g, +5 health, +5 mental)
Living: Old Sofa (30g), Nice Sofa (120g), TV (100g, +3 mental), Big Screen (400g, +5 mental), Game Console (150g, +4 mental)
Kitchen: Basic Stove (40g), Full Set (200g, +4 health), Chef Kitchen (600g, +charisma)
Study: Desk (50g), Bookshelf (80g, +empathy), Computer (200g, +2 mental), Library Wall (500g, +2 empathy)
Bathroom: Basic (30g), Nice (150g, +3 health)
Outdoor: Garden (60g, +empathy), BBQ (100g, +charisma), Pool (800g, +2 charisma, +5 mental)
Storage: Wine Rack (200g, +charisma)

### Gameplay Effects:
- Furniture health/mental bonuses applied each grid world (offsets aging decay)
- Furniture stat bonuses applied each grid world
- NPC visit impression based on total furniture value + fill rate
- Visit closeness bonus: empty=0, decent=+1, nice=+3, amazing=+5
- Integrated into BoardManager health decay step

### Files updated:
- NEW: Data/HousingSystem.cs
- PlayerData.cs: Added HousingSystem field
- BoardManager.cs: Housing health/stat bonuses applied in ProcessAge step 1