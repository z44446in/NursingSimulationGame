# Unity Game Structure Modification Request

**IMPORTANT: While the code structure should be described in English for proper implementation, please ensure that all field names, labels, and descriptions that appear in the Unity Inspector are implemented in Korean.**

## Current Structure
I currently have three data editors in my Unity game project:
- Nursing Action Data
- Interaction Data
- Procedure Data

These editors have too many overlapping functionalities, so I need to restructure them. I also want to modify the penalty system to make it more consistent and effective.

## Universal Requirements
1. All modifications must be configurable via the Inspector - every setting must be accessible through the Inspector panel
2. All game objects must follow "set native size" settings - this is mandatory for all visual elements
3. Object positioning must be done using Transform - no RectTransform adjustments or other positioning methods should be used

## Proposed New Data Structure

### 1. Interaction Data
This data editor will manage interactions and interaction stages that occur when items are clicked.

**Core Functionality:**
- Triggered when a specific item is called via the `pickUpItem` function
- Each interaction stage MUST have a guide message that appears in the guide panel text - this is mandatory for all stages
- Interaction stages are defined between interactions with:
  - Required sequence stages (with penalties for incorrect order)
  - Optional sequence stages (where order doesn't matter)
  - Each stage must be configurable via the "Add Stage" button in the Inspector
  - Each stage must have configurable ID, name, and penalty settings

**Main Interaction Types:**
- **Drag Interactions**
  - System must automatically generate direction arrows that blink gently to guide the user
  - Arrows must disappear when the user touches the screen
  - Arrow generation position and direction must be configurable in the Inspector
  - Must support both single-finger and two-finger drag operations:
    - Single finger drag option
    - Two-finger simultaneous drag option (can be same or different directions, configurable in Inspector)
  - Drag direction requirements must be configurable in Inspector
  - Drag recognition should only occur when touching and dragging the specified objects (objects are selected via Tags in Inspector)
  - Object movement during drag should support two types (configurable in Inspector):
    - Fixed movement: Tagged objects move in predefined direction and amount set in Inspector
    - Follow-drag movement: Objects follow drag directly, with penalties for moving outside specified boundary areas (boundary objects assigned in Inspector)
    - Collision detection with specific zone objects that trigger penalties

- **Object Creation**
  - Automatic generation of Inspector-specified game objects when a stage is triggered
  - Objects are managed via Tags in Inspector
  - Objects exist as inactive in hierarchy and should be activated with setActive
  - Created objects must be manipulable (via drag, click, etc.) in subsequent stages

- **Conditional Single Click**
  - Different outcomes (penalties or correct answers) based on which object is clicked
  - Objects are identified via Tags in the hierarchy
  - Different penalty settings for different tags (configurable in Inspector)

- **Sustained Single Click**
  - Requires continuous pressing until the stage is completed
  - Penalty applied if button is released early
  - Target game objects configured in Inspector (objects exist in hierarchy)

- **Object Deletion**
  - Automatic removal of specified objects when stage conditions are met
  - Objects selected via Tags in Inspector (objects exist in hierarchy)

- **Object Movement**
  - Automatic movement along predetermined paths when stage is triggered
  - Direction, path, and speed must be configurable in Inspector
  - Objects selected via Tags in Inspector (objects exist in hierarchy)

- **Quiz Popup**
  - Automatic generation of quiz prefab when stage is triggered
  - Quiz prefab must be linkable in Inspector
  - Must support configurable:
    - Text content
    - Images for option buttons
    - Number of choice buttons
    - Question text
    - Correct answer option selection
    - Time limit

- **Mini-game Popup**
  - Links to mini-game prefabs (configurable in Inspector)
  - Auto-starts the mini-game when stage is triggered
  - Mini-game details are managed within the mini-game itself (not in Interaction Data)

### 2. Procedure Data
Manages the sequence and checking of technical procedures.

**Procedure Types:**
- **Item Click** (via pickUpItem function)
  - Subsequent actions managed by Interaction Data
  - Must link seamlessly to the Interaction Data system

- **Scene "Action" Button Click**
  - When clicking the "Action" button in the scene, a panel appears with buttons
  - Correct button(s) must be configurable in Inspector
  - Support for multiple correct buttons (up to two) - both must be pressed to advance
  - Penalties for incorrect button selections

**Sequence Management:**
- Must support critical sequences with required order (with penalties for violations)
- Must support flexible sequences where order doesn't matter within a specified range
- Incorrect sequence following results in penalties

### 3. Procedure Type
Identifies which technical procedure is currently active in the game.

- Links to the appropriate Procedure Data ScriptableObject
- Separates "guideline version" from "clinical version" of the same procedure
- This separation is critical for maintaining different versions of the same procedure

## Penalty System

**Penalty Mechanism:**
1. When penalties occur, `DialogueManager.ShowSmallDialogue` must be called to display the `smallDialoguePrefab`
2. The penalty message text must be displayed in this dialogue
3. Different speakers are used for different penalty situations, with corresponding speaker images
4. Penalties must be recorded to a "penalty database" with configurable messages
5. If no message is specified in the Inspector for database recording, no record should be made
6. Each penalty requires four configurable settings in the Inspector:
   - Penalty score
   - Speaker
   - Penalty message (shown to user)
   - Database message (recorded in database)
7. When penalties occur, the screen edge must flash red twice
8. Progression to the next stage must be blocked until the penalty is resolved

## Request Details
Please restructure the scripts to implement this new organization pattern. After implementation, thoroughly check for any reference dependencies that might need updating to prevent errors. The solution should maintain all game functionality while reorganizing the data structures as specified.

Make sure that all Inspector-visible elements have Korean labels and tooltips, while maintaining English variable names in the underlying code for programming consistency.
