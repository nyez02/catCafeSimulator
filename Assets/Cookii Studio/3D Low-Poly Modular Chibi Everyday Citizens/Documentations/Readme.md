# Free Modular Chibi Everyday Citizens for Unity

## Overview

Free Modular Chibi Everyday Citizens for Unity is a small low-poly modular character pack for cozy games, life simulators, social games, and casual mobile projects.

The package includes one stylized chibi base character, modular clothing parts, simple accessories, prefabs, materials, textures, and a demo scene.

## Tested Version

Unity 6000.3.12f1

## Render Pipeline

- Universal Render Pipeline (URP)

## Package Contents

- 1 modular chibi base character
- Modular clothing parts (Top_Clothes, Bottom_Clothes, Footwear, Hairstyle and Accessories)
- Character and part prefabs
- Materials and textures
- Demo scene

## Demo Scene

Please open the demo scene to see the intended setup and example character assembly.

Path to Demo Scene:

`Assets/Cozy Citizen Studio/DemoScene/Chibi_DemoScene.unity`

## Basic Setup

1. Import the package into a URP project.
2. Open the demo scene.
3. Drag the main character prefab from `Prefabs/Characters` into your scene.
4. Use the modular parts from `Prefabs/Parts` to customize the character.

## How to Use the Character

1. Drag the base character prefab into the scene.
2. Expand the prefab hierarchy.
3. Enable the clothing and accessory parts you want to use.
4. Disable the parts you do not want to use.
5. Save the final setup as a prefab variant if needed.

## How to Swap Clothing

1. Select the character prefab in the scene.
2. Open the character hierarchy.
3. Find the clothing parts you want to change.
4. Disable the current clothing mesh.
5. Enable the new clothing mesh.
6. Check the result from different views.

## How to Attach Accessories

Accessories such as the hat and headphones should be attached to the head bone of the character.

1. Drag the accessory prefab into the scene.
2. Expand the character armature hierarchy.
3. Parent the accessory to the head bone. (`ARM_Chibi_Base/DEF_Spine/DEF_Head`)
4. If your rig uses `DEF_Head`, attach the accessory to `DEF_Head`.
5. Reset the accessory local transform if needed.
6. Adjust local position, rotation, and scale slightly for the best fit.

Note:

- If your setup includes an accessory anchor under the head bone, use that anchor instead.
- If not, attach directly to the head bone.

## Hoodie Setup and Hidden Body Parts

Some clothing pieces such as hoodies may cover parts of the base body mesh.

To reduce mesh clipping:

1. Enable the hoodie mesh.
2. Find the arm meshes in the character hierarchy.
3. Disable the arm meshes that are fully covered by the hoodie.
4. Keep only the visible body parts active.
5. Check the character from different angles.

This helps reduce mesh overlap and gives a cleaner final result.

## URP Material Setup

This package uses the `Universal Render Pipeline/Lit` shader.

### Material Setup

1. Open the material in the Inspector.
2. Set the shader to:

   `Universal Render Pipeline/Lit`
3. In **Surface Options**, set:

   -**Workflow Mode**: Metallic

   -**Surface Type**: Opaque
4. In **Surface Inputs**:

   - Assign the Base Color texture  to **Base Map**
   - Assign the Metallic_Smoothness texture  to **Metallic Map**
5. Keep **Smoothness Source** set to:

   -**Metallic Alpha**

### Texture Name

This package uses a simple texture setup:

-**Base Color** (TEX_Chibi_$meshname_BC)

-**Metallic_Smoothness** (TEX_Chibi_$meshname_M_SM)

### Metallic_Smoothness Import Settings

1. Select the `Metallic_Smoothness` texture in the Project window.
2. In the Inspector, keep **Texture Type** as **Default**.
3. Disable **sRGB (Color Texture)**.
4. Click **Apply**.

## Recommended Workflow

- Use the prepared prefabs instead of editing raw source models directly.
- Create the final character setup in the scene.
- Check clothing and accessories carefully.
- Save the customized result as a prefab variant if needed.

## Notes

- This package includes rigging only.
- Animations are not included.
- This package is intended as a small starter NPC kit.
- Some accessories may need small transform adjustments depending on the hairstyle or outfit.
- Some clothing combinations may require hidden body parts to avoid overlap.

## Support

Publisher: Cozy Citizen Studio
