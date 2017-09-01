﻿using System;
using System.Collections.Generic;
using Assets.Scripts.Common;
using Assets.Sources.Scripts.UI.Common;
using FF9;
using Memoria;
using Memoria.Assets;
using Memoria.Data;
using UnityEngine;
using Object = System.Object;

public class EquipUI : UIScene
{
	public Int32 CurrentPartyIndex
	{
		set
		{
			this.currentPartyIndex = value;
		}
	}

	public override void Show(UIScene.SceneVoidDelegate afterFinished = null)
	{
		UIScene.SceneVoidDelegate sceneVoidDelegate = delegate
		{
			PersistenSingleton<UIManager>.Instance.MainMenuScene.SubMenuPanel.SetActive(false);
			ButtonGroupState.SetPointerOffsetToGroup(new Vector2(10f, 0f), EquipUI.InventoryGroupButton);
			ButtonGroupState.SetScrollButtonToGroup(this.equipSelectScrollList.ScrollButton, EquipUI.InventoryGroupButton);
			ButtonGroupState.SetPointerLimitRectToGroup(new Vector4(-7f, -49f, 745f, 325f), EquipUI.InventoryGroupButton);
			ButtonGroupState.ActiveGroup = EquipUI.SubMenuGroupButton;
		};
		if (afterFinished != null)
		{
			sceneVoidDelegate = (UIScene.SceneVoidDelegate)Delegate.Combine(sceneVoidDelegate, afterFinished);
		}
		SceneDirector.FadeEventSetColor(FadeMode.Sub, Color.black);
		base.Show(sceneVoidDelegate);
		this.SwitchCharacter(true);
		this.DisplaySubMenuArrow(true);
		if (FF9StateSystem.PCPlatform)
		{
			this.HelpDespLabelGameObject.SetActive(true);
		}
		else
		{
			this.HelpDespLabelGameObject.SetActive(false);
		}
		this.equipSelectScrollList.ScrollButton.DisplayScrollButton(false, false);
	}

	public override void Hide(UIScene.SceneVoidDelegate afterFinished = null)
	{
		base.Hide(afterFinished);
		if (!this.fastSwitch)
		{
			PersistenSingleton<UIManager>.Instance.MainMenuScene.StartSubmenuTweenIn();
			this.RemoveCursorMemorize();
		}
	}

	private void RemoveCursorMemorize()
	{
		ButtonGroupState.RemoveCursorMemorize(EquipUI.SubMenuGroupButton);
		ButtonGroupState.RemoveCursorMemorize(EquipUI.EquipmentGroupButton);
	}

	public override Boolean OnKeyConfirm(GameObject go)
	{
		if (base.OnKeyConfirm(go))
		{
			if (ButtonGroupState.ActiveGroup == EquipUI.SubMenuGroupButton)
			{
				this.currentMenu = this.GetSubMenuFromGameObject(go);
				switch (this.currentMenu)
				{
				case EquipUI.SubMenu.Equip:
				case EquipUI.SubMenu.Off:
					FF9Sfx.FF9SFX_Play(103);
					ButtonGroupState.ActiveGroup = EquipUI.EquipmentGroupButton;
					ButtonGroupState.SetSecondaryOnGroup(EquipUI.SubMenuGroupButton);
					ButtonGroupState.HoldActiveStateOnGroup(EquipUI.SubMenuGroupButton);
					this.DisplaySubMenuArrow(false);
					this.DisplayEquiptmentInfo();
					break;
				case EquipUI.SubMenu.Optimize:
					this.EquipStrongest();
					this.DisplayPlayer(true);
					this.DisplayParameter();
					this.DisplayEquipment();
					break;
				}
			}
			else if (ButtonGroupState.ActiveGroup == EquipUI.EquipmentGroupButton)
			{
				if (ButtonGroupState.ContainButtonInGroup(go, EquipUI.EquipmentGroupButton))
				{
					this.currentEquipPart = go.transform.GetSiblingIndex();
					if (this.currentMenu == EquipUI.SubMenu.Off)
					{
						Boolean flag = false;
						if (this.currentEquipPart != 0)
						{
							PLAYER player = FF9StateSystem.Common.FF9.party.member[this.currentPartyIndex];
							Int32 num = (Int32)player.equip[this.currentEquipPart];
							if (num != 255)
							{
								FF9Sfx.FF9SFX_Play(107);
								if (num == 229)
								{
									ff9item.DecreaseMoonStoneCount();
								}
								ff9item.FF9Item_Add(num, 1);
							    player.equip[this.currentEquipPart] = CharacterEquipment.EmptyItemId;
								this.UpdateCharacterData(player);
								flag = true;
							}
							else
							{
								FF9Sfx.FF9SFX_Play(102);
							}
						}
						else
						{
							FF9Sfx.FF9SFX_Play(102);
						}
						this.DisplayParameter();
						this.DisplayEquipment();
						this.DisplayHelp();
						if (flag)
						{
							ButtonGroupState.RefreshHelpDialog();
						}
					}
					else
					{
						FF9Sfx.FF9SFX_Play(103);
						this.DisplayPlayerArrow(false);
						this.DisplayInventory();
						String key = String.Empty;
						switch (this.currentEquipPart)
						{
						case 0:
							key = "WeaponCaption";
							break;
						case 1:
							key = "HeadCaption";
							break;
						case 2:
							key = "WristCaption";
							break;
						case 3:
							key = "ArmorCaption";
							break;
						default:
							key = "AccessoryCaption";
							break;
						}
						this.selectedCaption.key = key;
						base.Loading = true;
						this.equipmentSelectionTransition.TweenIn(new Byte[1], delegate
						{
							base.Loading = false;
							ButtonGroupState.RemoveCursorMemorize(EquipUI.InventoryGroupButton);
							ButtonGroupState.ActiveGroup = EquipUI.InventoryGroupButton;
							this.equipmentPartCaption.SetActive(false);
							this.currentItemIndex = 0;
							this.DisplayParameter();
							this.DisplayEquiptmentInfo();
							this.DisplayHelp();
						});
					}
				}
				else
				{
					this.OnSecondaryGroupClick(go);
				}
			}
			else if (ButtonGroupState.ActiveGroup == EquipUI.InventoryGroupButton && ButtonGroupState.ContainButtonInGroup(go, EquipUI.InventoryGroupButton))
			{
				this.currentItemIndex = go.GetComponent<RecycleListItem>().ItemDataIndex;
				if (this.itemIdList[this.currentEquipPart][this.currentItemIndex].id != 255)
				{
					FF9Sfx.FF9SFX_Play(107);
					PLAYER player2 = FF9StateSystem.Common.FF9.party.member[this.currentPartyIndex];
					Int32 num2 = (Int32)player2.equip[this.currentEquipPart];
					Int32 id = (Int32)this.itemIdList[this.currentEquipPart][this.currentItemIndex].id;
					if (num2 != 255)
					{
						if (num2 == 229)
						{
							ff9item.DecreaseMoonStoneCount();
						}
						ff9item.FF9Item_Add(num2, 1);
					}
					if (ff9item.FF9Item_Remove(id, 1) != 0)
					{
						player2.equip[this.currentEquipPart] = (Byte)id;
						this.UpdateCharacterData(player2);
						this.DisplayEquipment();
						this.DisplayParameter();
					}
					this.ClearChangeParameter();
					this.DisplayPlayerArrow(true);
					ButtonGroupState.ActiveGroup = EquipUI.EquipmentGroupButton;
					base.Loading = true;
					this.equipmentSelectionTransition.TweenOut(new Byte[1], delegate
					{
						base.Loading = false;
					});
					this.equipmentPartCaption.SetActive(true);
				}
				else
				{
					FF9Sfx.FF9SFX_Play(102);
				}
			}
		}
		return true;
	}

	public override Boolean OnKeyCancel(GameObject go)
	{
		if (base.OnKeyCancel(go))
		{
			if (ButtonGroupState.ActiveGroup == EquipUI.SubMenuGroupButton)
			{
				FF9Sfx.FF9SFX_Play(101);
				this.fastSwitch = false;
				this.Hide(delegate
				{
					PersistenSingleton<UIManager>.Instance.MainMenuScene.NeedTweenAndHideSubMenu = false;
					PersistenSingleton<UIManager>.Instance.MainMenuScene.CurrentSubMenu = MainMenuUI.SubMenu.Equip;
					PersistenSingleton<UIManager>.Instance.ChangeUIState(UIManager.UIState.MainMenu);
				});
			}
			else if (ButtonGroupState.ActiveGroup == EquipUI.EquipmentGroupButton)
			{
				FF9Sfx.FF9SFX_Play(101);
				ButtonGroupState.ActiveGroup = EquipUI.SubMenuGroupButton;
				this.ClearChangeParameter();
				this.ClearEquipmentInfo();
				this.DisplaySubMenuArrow(true);
			}
			else if (ButtonGroupState.ActiveGroup == EquipUI.InventoryGroupButton)
			{
				FF9Sfx.FF9SFX_Play(101);
				this.DisplayPlayerArrow(true);
				this.ClearChangeParameter();
				ButtonGroupState.ActiveGroup = EquipUI.EquipmentGroupButton;
				this.DisplayEquiptmentInfo();
				base.Loading = true;
				this.equipmentSelectionTransition.TweenOut(new Byte[1], delegate
				{
					base.Loading = false;
				});
				this.equipmentPartCaption.SetActive(true);
			}
		}
		return true;
	}

	public override Boolean OnItemSelect(GameObject go)
	{
		if (base.OnItemSelect(go))
		{
			if (ButtonGroupState.ActiveGroup == EquipUI.SubMenuGroupButton && this.currentMenu != this.GetSubMenuFromGameObject(go))
			{
				this.currentMenu = this.GetSubMenuFromGameObject(go);
				this.DisplayEquipment();
			}
			if (ButtonGroupState.ActiveGroup == EquipUI.EquipmentGroupButton)
			{
				if (this.currentEquipPart != go.transform.GetSiblingIndex())
				{
					this.currentEquipPart = go.transform.GetSiblingIndex();
					this.DisplayEquiptmentInfo();
					this.DisplayParameter();
				}
			}
			else if (ButtonGroupState.ActiveGroup == EquipUI.InventoryGroupButton && this.currentItemIndex != go.GetComponent<RecycleListItem>().ItemDataIndex)
			{
				this.currentItemIndex = go.GetComponent<RecycleListItem>().ItemDataIndex;
				this.DisplayEquiptmentInfo();
				this.DisplayParameter();
			}
		}
		return true;
	}

	public override Boolean OnKeySpecial(GameObject go)
	{
		if (base.OnKeySpecial(go) && ButtonGroupState.ActiveGroup == EquipUI.SubMenuGroupButton)
		{
			FF9Sfx.FF9SFX_Play(103);
			this.fastSwitch = true;
			this.Hide(delegate
			{
				this.RemoveCursorMemorize();
				PersistenSingleton<UIManager>.Instance.AbilityScene.CurrentPartyIndex = this.currentPartyIndex;
				PersistenSingleton<UIManager>.Instance.ChangeUIState(UIManager.UIState.Ability);
			});
		}
		return true;
	}

	public override Boolean OnKeyLeftBumper(GameObject go)
	{
		if (base.OnKeyLeftBumper(go) && (ButtonGroupState.ActiveGroup == EquipUI.SubMenuGroupButton || ButtonGroupState.ActiveGroup == EquipUI.EquipmentGroupButton) && this.CharacterArrowPanel.activeSelf)
		{
			FF9Sfx.FF9SFX_Play(1047);
			Int32 num = ff9play.FF9Play_GetPrev(this.currentPartyIndex);
			if (num != this.currentPartyIndex)
			{
				this.currentPartyIndex = num;
				PLAYER player = FF9StateSystem.Common.FF9.party.member[this.currentPartyIndex];
				String spritName = FF9UIDataTool.AvatarSpriteName(player.info.serial_no);
				base.Loading = true;
				Boolean isKnockOut = player.cur.hp == 0;
				this.avatarTransition.Change(spritName, HonoAvatarTweenPosition.Direction.LeftToRight, isKnockOut, delegate
				{
					this.DisplayPlayer(true);
					base.Loading = false;
				});
				this.SwitchCharacter(false);
			}
		}
		return true;
	}

	public override Boolean OnKeyRightBumper(GameObject go)
	{
		if (base.OnKeyRightBumper(go) && (ButtonGroupState.ActiveGroup == EquipUI.SubMenuGroupButton || ButtonGroupState.ActiveGroup == EquipUI.EquipmentGroupButton) && this.CharacterArrowPanel.activeSelf)
		{
			FF9Sfx.FF9SFX_Play(1047);
			Int32 num = ff9play.FF9Play_GetNext(this.currentPartyIndex);
			if (num != this.currentPartyIndex)
			{
				this.currentPartyIndex = num;
				PLAYER player = FF9StateSystem.Common.FF9.party.member[this.currentPartyIndex];
				String spritName = FF9UIDataTool.AvatarSpriteName(player.info.serial_no);
				Boolean isKnockOut = player.cur.hp == 0;
				this.avatarTransition.Change(spritName, HonoAvatarTweenPosition.Direction.RightToLeft, isKnockOut, delegate
				{
					this.DisplayPlayer(true);
					base.Loading = false;
				});
				this.SwitchCharacter(false);
				base.Loading = true;
			}
		}
		return true;
	}

	private void OnSecondaryGroupClick(GameObject go)
	{
		ButtonGroupState.HoldActiveStateOnGroup(go, EquipUI.SubMenuGroupButton);
		if (ButtonGroupState.ActiveGroup == EquipUI.EquipmentGroupButton)
		{
			FF9Sfx.muteSfx = true;
			this.OnKeyCancel(this.equipmentHud.Self.GetChild(this.currentEquipPart));
			FF9Sfx.muteSfx = false;
			this.OnKeyConfirm(go);
		}
	}

	private void DisplayHelp()
	{
		PLAYER player = FF9StateSystem.Common.FF9.party.member[this.currentPartyIndex];
		Int32 num = 0;
        CharacterEquipment equip = player.equip;
		for (Int32 i = 0; i < CharacterEquipment.Length; i++)
		{
			Byte itemId = equip[i];
			ButtonGroupState buttonGroupState = (ButtonGroupState)null;
			switch (num)
			{
			case 0:
				buttonGroupState = this.equipmentHud.Weapon.Button;
				break;
			case 1:
				buttonGroupState = this.equipmentHud.Head.Button;
				break;
			case 2:
				buttonGroupState = this.equipmentHud.Wrist.Button;
				break;
			case 3:
				buttonGroupState = this.equipmentHud.Body.Button;
				break;
			case 4:
				buttonGroupState = this.equipmentHud.Accessory.Button;
				break;
			}
			if (itemId != 255)
			{
				buttonGroupState.Help.TextKey = String.Empty;
				buttonGroupState.Help.Text = FF9TextTool.ItemHelpDescription(itemId);
			}
			else
			{
				buttonGroupState.Help.TextKey = Localization.Get("NoEquipHelp");
			}
			num++;
		}
	}

	private void DisplaySubMenuArrow(Boolean isEnable)
	{
		if (isEnable)
		{
			if (FF9StateSystem.PCPlatform)
			{
				this.submenuArrowGameObject.SetActive(false);
			}
			else
			{
				this.submenuArrowGameObject.SetActive(true);
			}
		}
		else
		{
			this.submenuArrowGameObject.SetActive(false);
		}
	}

	private void DisplayPlayerArrow(Boolean isEnable)
	{
		if (isEnable)
		{
			Int32 num = 0;
			PLAYER[] member = FF9StateSystem.Common.FF9.party.member;
			for (Int32 i = 0; i < (Int32)member.Length; i++)
			{
				PLAYER player = member[i];
				if (player != null)
				{
					num++;
				}
			}
			if (num > 1)
			{
				this.CharacterArrowPanel.SetActive(true);
			}
			else
			{
				this.CharacterArrowPanel.SetActive(false);
			}
		}
		else
		{
			this.CharacterArrowPanel.SetActive(false);
		}
	}

	private void DisplayPlayer(Boolean updateAvatar)
	{
		PLAYER player = FF9StateSystem.Common.FF9.party.member[this.currentPartyIndex];
		FF9UIDataTool.DisplayCharacterDetail(player, this.characterHud);
		if (updateAvatar)
		{
			FF9UIDataTool.DisplayCharacterAvatar(player, default(Vector3), default(Vector3), this.characterHud.AvatarSprite, false);
		}
	}

	private void DisplayParameter()
	{
		PLAYER player = FF9StateSystem.Common.FF9.party.member[this.currentPartyIndex];
		this.parameterHud.ParameterLabel[0].text = player.elem.dex.ToString();
		this.parameterHud.ParameterLabel[1].text = player.elem.str.ToString();
		this.parameterHud.ParameterLabel[2].text = player.elem.mgc.ToString();
		this.parameterHud.ParameterLabel[3].text = player.elem.wpr.ToString();
		this.parameterHud.ParameterLabel[4].text = ff9weap.WeaponData[(Int32)player.equip[0]].Ref.Power.ToString();
		this.parameterHud.ParameterLabel[5].text = player.defence.PhisicalDefence.ToString();
		this.parameterHud.ParameterLabel[6].text = player.defence.PhisicalEvade.ToString();
		this.parameterHud.ParameterLabel[7].text = player.defence.MagicalDefence.ToString();
		this.parameterHud.ParameterLabel[8].text = player.defence.MagicalEvade.ToString();
		if (ButtonGroupState.ActiveGroup == EquipUI.InventoryGroupButton || (ButtonGroupState.ActiveGroup == EquipUI.EquipmentGroupButton && this.currentMenu == EquipUI.SubMenu.Off))
		{
			if (ButtonGroupState.ActiveGroup == EquipUI.EquipmentGroupButton && this.currentMenu == EquipUI.SubMenu.Off)
			{
				if (player.equip[this.currentEquipPart] == 255)
				{
					this.ClearChangeParameter();
					return;
				}
				if (this.currentEquipPart == 0)
				{
					this.ClearChangeParameter();
					return;
				}
			}
			else if (ButtonGroupState.ActiveGroup == EquipUI.InventoryGroupButton && (this.currentItemIndex == -1 || this.itemIdList[this.currentEquipPart][this.currentItemIndex].id == 255))
			{
				this.ClearChangeParameter();
				return;
			}
			for (Int32 i = 0; i < 9; i++)
			{
				this.parameterHud.ArrowSprite[i].gameObject.SetActive(true);
				this.parameterHud.NewParameterLabel[i].gameObject.SetActive(true);
			}
			FF9PLAY_SKILL ff9PLAY_SKILL = new FF9PLAY_SKILL();
			FF9PLAY_SKILL ff9PLAY_SKILL2 = this.CalculateSkill();
			ff9PLAY_SKILL.Base[0] = player.elem.dex;
			ff9PLAY_SKILL.Base[1] = player.elem.str;
			ff9PLAY_SKILL.Base[2] = player.elem.mgc;
			ff9PLAY_SKILL.Base[3] = player.elem.wpr;
			ff9PLAY_SKILL.weapon[0] = (UInt16)ff9weap.WeaponData[(Int32)player.equip[0]].Ref.Power;
			ff9PLAY_SKILL.weapon[1] = (UInt16)player.defence.PhisicalDefence;
			ff9PLAY_SKILL.weapon[2] = (UInt16)player.defence.PhisicalEvade;
			ff9PLAY_SKILL.weapon[3] = (UInt16)player.defence.MagicalDefence;
			ff9PLAY_SKILL.weapon[4] = (UInt16)player.defence.MagicalEvade;
			Int32 j;
			for (j = 0; j < 4; j++)
			{
				this.parameterHud.NewParameterLabel[j].text = ff9PLAY_SKILL2.Base[j].ToString();
			}
			while (j < 9)
			{
				this.parameterHud.NewParameterLabel[j].text = ff9PLAY_SKILL2.weapon[j - 4].ToString();
				j++;
			}
			for (j = 0; j < 4; j++)
			{
				if (ff9PLAY_SKILL.Base[j] != ff9PLAY_SKILL2.Base[j])
				{
					this.parameterHud.ArrowTween[j].enabled = true;
				}
				else
				{
					this.parameterHud.ArrowTween[j].enabled = false;
					this.parameterHud.ArrowSprite[j].color = new Color(1f, 1f, 1f, 0.7058824f);
				}
			}
			while (j < 9)
			{
				if (ff9PLAY_SKILL.weapon[j - 4] != ff9PLAY_SKILL2.weapon[j - 4])
				{
					this.parameterHud.ArrowTween[j].enabled = true;
				}
				else
				{
					this.parameterHud.ArrowTween[j].enabled = false;
					this.parameterHud.ArrowSprite[j].color = new Color(1f, 1f, 1f, 0.7058824f);
				}
				j++;
			}
			for (j = 0; j < 4; j++)
			{
				if (ff9PLAY_SKILL.Base[j] < ff9PLAY_SKILL2.Base[j])
				{
					this.parameterHud.NewParameterLabel[j].color = FF9TextTool.Green;
				}
				else if (ff9PLAY_SKILL.Base[j] > ff9PLAY_SKILL2.Base[j])
				{
					this.parameterHud.NewParameterLabel[j].color = FF9TextTool.Red;
				}
				else
				{
					this.parameterHud.NewParameterLabel[j].color = FF9TextTool.White;
				}
			}
			while (j < 9)
			{
				if (ff9PLAY_SKILL.weapon[j - 4] < ff9PLAY_SKILL2.weapon[j - 4])
				{
					this.parameterHud.NewParameterLabel[j].color = FF9TextTool.Green;
				}
				else if (ff9PLAY_SKILL.weapon[j - 4] > ff9PLAY_SKILL2.weapon[j - 4])
				{
					this.parameterHud.NewParameterLabel[j].color = FF9TextTool.Red;
				}
				else
				{
					this.parameterHud.NewParameterLabel[j].color = FF9TextTool.White;
				}
				j++;
			}
		}
		else
		{
			this.ClearChangeParameter();
		}
	}

	private void DisplayEquipment()
	{
		PLAYER player = FF9StateSystem.Common.FF9.party.member[this.currentPartyIndex];
		if (this.currentMenu == EquipUI.SubMenu.Off)
		{
			FF9UIDataTool.DisplayItem((Int32)player.equip[0], this.equipmentHud.Weapon.IconSprite, this.equipmentHud.Weapon.NameLabel, false);
			this.weaponPartColonLabel.color = FF9TextTool.Gray;
			this.weaponPartIconSprite.alpha = 0.5f;
		}
		else
		{
			FF9UIDataTool.DisplayItem((Int32)player.equip[0], this.equipmentHud.Weapon.IconSprite, this.equipmentHud.Weapon.NameLabel, true);
			this.weaponPartColonLabel.color = FF9TextTool.White;
			this.weaponPartIconSprite.alpha = 1f;
		}
		FF9UIDataTool.DisplayItem((Int32)player.equip[1], this.equipmentHud.Head.IconSprite, this.equipmentHud.Head.NameLabel, true);
		FF9UIDataTool.DisplayItem((Int32)player.equip[2], this.equipmentHud.Wrist.IconSprite, this.equipmentHud.Wrist.NameLabel, true);
		FF9UIDataTool.DisplayItem((Int32)player.equip[3], this.equipmentHud.Body.IconSprite, this.equipmentHud.Body.NameLabel, true);
		FF9UIDataTool.DisplayItem((Int32)player.equip[4], this.equipmentHud.Accessory.IconSprite, this.equipmentHud.Accessory.NameLabel, true);
		this.DisplayHelp();
	}

	private void DisplayEquiptmentInfo()
	{
		if (ButtonGroupState.ActiveGroup == EquipUI.EquipmentGroupButton || ButtonGroupState.ActiveGroup == EquipUI.InventoryGroupButton)
		{
			Character player = FF9StateSystem.Common.FF9.party.GetCharacter(this.currentPartyIndex);
			String spriteName = String.Empty;
			switch (this.currentEquipPart)
			{
			case 0:
				spriteName = "icon_equip_0";
				break;
			case 1:
				spriteName = "icon_equip_1";
				break;
			case 2:
				spriteName = "icon_equip_2";
				break;
			case 3:
				spriteName = "icon_equip_3";
				break;
			case 4:
				spriteName = "icon_equip_4";
				break;
			default:
				spriteName = String.Empty;
				break;
			}
			this.equipmentSelectHud.Self.SetActive(true);
			this.equipmentSelectHud.PartIconSprite.spriteName = spriteName;
			Int32 num;
			FF9ITEM_DATA ff9ITEM_DATA;
			if (ButtonGroupState.ActiveGroup == EquipUI.EquipmentGroupButton)
			{
				num = player.Equipment[this.currentEquipPart];
				ff9ITEM_DATA = ff9item._FF9Item_Data[num];
			}
			else
			{
				if (this.currentItemIndex == -1)
				{
					return;
				}
				num = (Int32)this.itemIdList[this.currentEquipPart][this.currentItemIndex].id;
				ff9ITEM_DATA = ff9item._FF9Item_Data[num];
			}
			FF9UIDataTool.DisplayItem(num, this.equipmentSelectHud.Equipment.IconSprite, this.equipmentSelectHud.Equipment.NameLabel, true);
			this.equipmentAbilitySelectHudList[0].Self.SetActive(false);
			this.equipmentAbilitySelectHudList[1].Self.SetActive(false);
			this.equipmentAbilitySelectHudList[2].Self.SetActive(false);
			Int32 num2 = 0;
			for (Int32 i = 0; i < 3; i++)
			{
				Int32 num3 = (Int32)ff9ITEM_DATA.ability[i];
				if (num3 != 0)
				{
					String text = String.Empty;
					if (num3 < 192)
					{
						text = FF9TextTool.ActionAbilityName(num3);
					}
					else
					{
						text = FF9TextTool.SupportAbilityName(num3 - 192);
					}
					this.equipmentAbilitySelectHudList[num2].Self.SetActive(true);
					if (ff9abil.FF9Abil_HasAp(player))
					{
						Boolean flag = ff9abil.FF9Abil_GetIndex(player.Index, num3) >= 0;
						Int32 num4 = ff9abil.FF9Abil_GetMax(player.Index, num3);
						String spriteName2;
						if (flag)
						{
							if (num3 < 192)
							{
								spriteName2 = "ability_stone";
							}
							else
							{
								spriteName2 = ((!ff9abil.FF9Abil_IsEnableSA(player.Data.sa, num3)) ? "skill_stone_off" : "skill_stone_on");
							}
						}
						else
						{
							spriteName2 = ((num3 >= 192) ? "skill_stone_null" : "ability_stone_null");
						}
						if (flag)
						{
							Boolean isShowText = num3 >= 192 || (FF9StateSystem.Battle.FF9Battle.aa_data[num3].Type & 2) == 0;
							this.equipmentAbilitySelectHudList[num2].APBar.Self.SetActive(true);
							FF9UIDataTool.DisplayAPBar(player.Data, num3, isShowText, this.equipmentAbilitySelectHudList[num2].APBar);
						}
						else
						{
							this.equipmentAbilitySelectHudList[num2].APBar.Self.SetActive(false);
						}
						this.equipmentAbilitySelectHudList[num2].NameLabel.text = text;
						this.equipmentAbilitySelectHudList[num2].IconSprite.spriteName = spriteName2;
						if (flag)
						{
							this.equipmentAbilitySelectHudList[num2].NameLabel.color = FF9TextTool.White;
						}
						else
						{
							this.equipmentAbilitySelectHudList[num2].NameLabel.color = FF9TextTool.Gray;
						}
					}
					else
					{
						String spriteName2;
						if (num3 < 192)
						{
							spriteName2 = "ability_stone_null";
						}
						else
						{
							spriteName2 = "skill_stone_null";
						}
						this.equipmentAbilitySelectHudList[num2].NameLabel.text = text;
						this.equipmentAbilitySelectHudList[num2].IconSprite.spriteName = spriteName2;
						this.equipmentAbilitySelectHudList[num2].NameLabel.color = FF9TextTool.Gray;
						this.equipmentAbilitySelectHudList[num2].APBar.Self.SetActive(false);
					}
					num2++;
				}
			}
		}
	}

	private void DisplayInventory()
	{
	    Character character = FF9StateSystem.Common.FF9.party.GetCharacter(this.currentPartyIndex);
        Int32 characterMask = (Int32)this.charMask[ff9play.FF9Play_GetCharID2(character.Index, character.IsSubCharacter)];
		Int32 equipSlotMask = (Int32)this.partMask[this.currentEquipPart];
	    switch (this.currentEquipPart)
		{
		case 0:
			this.equipmentListCaption.text = Localization.Get("WeaponCaption");
			break;
		case 1:
			this.equipmentListCaption.text = Localization.Get("HeadCaption");
			break;
		case 2:
			this.equipmentListCaption.text = Localization.Get("WristCaption");
			break;
		case 3:
			this.equipmentListCaption.text = Localization.Get("ArmorCaption");
			break;
		case 4:
			this.equipmentListCaption.text = Localization.Get("AccessoryCaption");
			break;
		default:
			this.equipmentListCaption.text = Localization.Get("WeaponCaption");
			break;
		}
		List<FF9ITEM> list = this.itemIdList[this.currentEquipPart];
		list.Clear();
        int resultIndex = 0;
        for (Int32 i = 0; i < 256; i++)
		{
			if (FF9StateSystem.Common.FF9.item[i].count > 0)
			{
			    FF9ITEM item = FF9StateSystem.Common.FF9.item[i];
                FF9ITEM_DATA itemData = ff9item._FF9Item_Data[item.id];

				if (CanEquip(item, itemData, character, characterMask, equipSlotMask))
				{
                    this.tempItemList[resultIndex].id = item.id;
                    this.tempItemList[resultIndex].count = item.count;
                    list.Add(this.tempItemList[resultIndex]);
                    resultIndex++;
                }
			}
		}
		for (Int32 j = 0; j < list.Count - 1; j++)
		{
			for (Int32 k = j + 1; k < list.Count; k++)
			{
				FF9ITEM_DATA ff9ITEM_DATA2 = ff9item._FF9Item_Data[(Int32)list[j].id];
				FF9ITEM_DATA ff9ITEM_DATA3 = ff9item._FF9Item_Data[(Int32)list[k].id];
				if ((ff9ITEM_DATA2.eq_lv == ff9ITEM_DATA3.eq_lv && list[j].id > list[k].id) || ff9ITEM_DATA2.eq_lv > ff9ITEM_DATA3.eq_lv)
				{
					Byte id = list[j].id;
					Byte count = list[j].count;
					list[j].id = list[k].id;
					list[j].count = list[k].count;
					list[k].id = id;
					list[k].count = count;
				}
			}
		}
		if (list.Count == 0)
		{
			list.Add(new FF9ITEM(Byte.MaxValue, 0));
		}
		List<ListDataTypeBase> list2 = new List<ListDataTypeBase>();
		foreach (FF9ITEM itemData in list)
		{
			list2.Add(new EquipUI.EquipInventoryListData
			{
				ItemData = itemData
			});
		}
		if (this.equipSelectScrollList.ItemsPool.Count == 0)
		{
			this.equipSelectScrollList.PopulateListItemWithData = new Action<Transform, ListDataTypeBase, Int32, Boolean>(this.DisplayInventoryInfo);
			this.equipSelectScrollList.OnRecycleListItemClick += this.OnListItemClick;
			this.equipSelectScrollList.InitTableView(list2, 0);
		}
		else
		{
			this.equipSelectScrollList.SetOriginalData(list2);
			this.equipSelectScrollList.JumpToIndex(0, false);
		}
	}

    private Boolean CanEquip(FF9ITEM item, FF9ITEM_DATA itemData, Character character, Int32 characterMask, Int32 equipSlotMask)
    {
        const Int32 accessories = 4;
        const Int32 saveTheQueen = 26;

        if ((itemData.equip & characterMask) == 0)
            return false;

        if (this.currentEquipPart == accessories)
        {
            if (character.Index == CharacterIndex.Beatrix && item.id == saveTheQueen)
                return true;
        }

        return (itemData.type & equipSlotMask) != 0;
    }

    private void DisplayInventoryInfo(Transform item, ListDataTypeBase data, Int32 index, Boolean isInit)
	{
		EquipUI.EquipInventoryListData equipInventoryListData = (EquipUI.EquipInventoryListData)data;
		ItemListDetailWithIconHUD itemListDetailWithIconHUD = new ItemListDetailWithIconHUD(item.gameObject, true);
		if (isInit)
		{
			this.DisplayWindowBackground(item.gameObject, (UIAtlas)null);
			itemListDetailWithIconHUD.Button.Help.Enable = false;
		}
		if (equipInventoryListData.ItemData.id != 255)
		{
			ButtonGroupState.SetButtonAnimation(itemListDetailWithIconHUD.Self, true);
			itemListDetailWithIconHUD.IconSprite.gameObject.SetActive(true);
			itemListDetailWithIconHUD.NameLabel.gameObject.SetActive(true);
			itemListDetailWithIconHUD.NumberLabel.gameObject.SetActive(true);
			FF9UIDataTool.DisplayItem((Int32)equipInventoryListData.ItemData.id, itemListDetailWithIconHUD.IconSprite, itemListDetailWithIconHUD.NameLabel, true);
			itemListDetailWithIconHUD.NumberLabel.text = equipInventoryListData.ItemData.count.ToString();
			itemListDetailWithIconHUD.Button.Help.Enable = true;
			itemListDetailWithIconHUD.Button.Help.TextKey = String.Empty;
			itemListDetailWithIconHUD.Button.Help.Text = FF9TextTool.ItemHelpDescription((Int32)equipInventoryListData.ItemData.id);
		}
		else
		{
			ButtonGroupState.SetButtonAnimation(itemListDetailWithIconHUD.Self, false);
			itemListDetailWithIconHUD.IconSprite.gameObject.SetActive(false);
			itemListDetailWithIconHUD.NameLabel.gameObject.SetActive(false);
			itemListDetailWithIconHUD.NumberLabel.gameObject.SetActive(false);
			itemListDetailWithIconHUD.Button.Help.Enable = false;
			itemListDetailWithIconHUD.Button.Help.TextKey = String.Empty;
			itemListDetailWithIconHUD.Button.Help.Text = String.Empty;
		}
	}

	private void ClearEquipmentInfo()
	{
		this.equipmentSelectHud.Self.SetActive(false);
		this.equipmentAbilitySelectHudList[0].Self.SetActive(false);
		this.equipmentAbilitySelectHudList[1].Self.SetActive(false);
		this.equipmentAbilitySelectHudList[2].Self.SetActive(false);
	}

	private void ClearChangeParameter()
	{
		for (Int32 i = 0; i < 9; i++)
		{
			this.parameterHud.ArrowSprite[i].gameObject.SetActive(false);
			this.parameterHud.NewParameterLabel[i].gameObject.SetActive(false);
		}
	}

	private void InitialData()
	{
		this.ClearEquipmentInfo();
	}

	private void SwitchCharacter(Boolean updateAvatar)
	{
		this.InitialData();
		this.DisplayPlayerArrow(true);
		this.DisplayPlayer(updateAvatar);
		this.DisplayParameter();
		this.DisplayEquipment();
		this.DisplayEquiptmentInfo();
	}

	private void UpdateCharacterData(PLAYER player)
	{
		this.UpdateCharacterSA(player);
		ff9play.FF9Play_Update(player);
		player.info.serial_no = (Byte)ff9play.FF9Play_GetSerialID(player.info.slot_no, (player.category & 16) != 0, player.equip);
	}

	private void UpdateCharacterSA(PLAYER player)
	{
		List<Int32> list = new List<Int32>();
		if (!ff9abil.FF9Abil_HasAp(new Character(player)))
		{
			return;
		}
		for (Int32 i = 0; i < 5; i++)
		{
			if (player.equip[i] != 255)
			{
				FF9ITEM_DATA ff9ITEM_DATA = ff9item._FF9Item_Data[(Int32)player.equip[i]];
				for (Int32 j = 0; j < 3; j++)
				{
					Int32 num = (Int32)ff9ITEM_DATA.ability[j];
					if (num != 0 && 192 <= num)
					{
						list.Add(num - 192);
					}
				}
			}
		}
		CharacterAbility[] array = ff9abil._FF9Abil_PaData[player.PresetId];
		for (Int32 k = 0; k < 48; k++)
		{
			if (192 <= array[k].Id && ff9abil.FF9Abil_GetEnableSA(player.Index, array[k].Id) && !list.Contains(array[k].Id - 192) && player.pa[k] < array[k].Ap)
			{
				ff9abil.FF9Abil_SetEnableSA(player.Index, array[k].Id, false);
				Byte capa_val = ff9abil._FF9Abil_SaData[array[k].Id - 192].GemsCount;
				if (player.max.capa - player.cur.capa >= capa_val)
				{
					POINTS cur = player.cur;
					cur.capa = (Byte)(cur.capa + capa_val);
				}
				else
				{
					player.cur.capa = player.max.capa;
				}
			}
		}
	}

	private void EquipStrongest()
	{
		PLAYER player = FF9StateSystem.Common.FF9.party.member[this.currentPartyIndex];
		Int32 itemId = 0;
		Int32 num2 = (CharacterId)ff9play.FF9Play_GetCharID2(player.Index, player.IsSubCharacter);
		for (Int32 i = 0; i < 5; i++)
		{
			Int32 num3 = (Int32)((player.equip[i] == Byte.MaxValue) ? -1 : ((Int32)ff9item._FF9Item_Data[(Int32)player.equip[i]].eq_lv));
			Int32 num4 = -1;
			FF9ITEM ff9ITEM = FF9StateSystem.Common.FF9.item[0];
			for (Int32 j = 0; j < 256; j++)
			{
				if (ff9ITEM.count > 0)
				{
					FF9ITEM_DATA ff9ITEM_DATA = ff9item._FF9Item_Data[(Int32)ff9ITEM.id];
					if ((ff9ITEM_DATA.type & this.partMask[i]) != 0 && (ff9ITEM_DATA.equip & this.charMask[num2]) != 0 && (Int32)ff9ITEM_DATA.eq_lv > num4)
					{
						num4 = (Int32)ff9ITEM_DATA.eq_lv;
						itemId = (Int32)ff9ITEM.id;
					}
				}
				ff9ITEM = FF9StateSystem.Common.FF9.item[j];
			}
			if (num4 > num3)
			{
				if (player.equip[i] != 255)
				{
					if (player.equip[i] == 229)
					{
						ff9item.DecreaseMoonStoneCount();
					}
					ff9item.FF9Item_Add((Int32)player.equip[i], 1);
				}
				if (ff9item.FF9Item_Remove(itemId, 1) != 0)
				{
					player.equip[i] = (Byte)itemId;
				}
			}
		}
		this.UpdateCharacterData(player);
		FF9Sfx.FF9SFX_Play(107);
	}

	private FF9PLAY_SKILL CalculateSkill()
	{
		PLAYER player = FF9StateSystem.Common.FF9.party.member[this.currentPartyIndex];
		FF9PLAY_INFO ff9PLAY_INFO = new FF9PLAY_INFO();
		FF9PLAY_SKILL result = new FF9PLAY_SKILL();
		Byte itemId;
		if (ButtonGroupState.ActiveGroup == EquipUI.InventoryGroupButton)
		{
			itemId = this.itemIdList[this.currentEquipPart][this.currentItemIndex].id;
		}
		else
		{
			itemId = player.equip[this.currentEquipPart];
		}
		ff9PLAY_INFO.Base = player.basis;
		ff9PLAY_INFO.cur_hp = player.cur.hp;
		ff9PLAY_INFO.cur_mp = (UInt16)player.cur.mp;
        ff9PLAY_INFO.equip.Absorb(player.equip);
        for (Int32 j = 0; j < (Int32)player.sa.Length; j++)
		{
			ff9PLAY_INFO.sa[j] = player.sa[j];
		}
		if (ButtonGroupState.ActiveGroup == EquipUI.InventoryGroupButton)
		{
			ff9PLAY_INFO.equip[this.currentEquipPart] = itemId;
			ff9play.FF9Play_GetSkill(ref ff9PLAY_INFO, ref result);
		}
		else if (this.currentMenu == EquipUI.SubMenu.Off && ButtonGroupState.ActiveGroup == EquipUI.EquipmentGroupButton && this.currentEquipPart != 0)
		{
		    ff9PLAY_INFO.equip[this.currentEquipPart] = CharacterEquipment.EmptyItemId;
			ff9play.FF9Play_GetSkill(ref ff9PLAY_INFO, ref result);
		}
		return result;
	}

	public EquipUI.SubMenu GetSubMenuFromGameObject(GameObject go)
	{
		if (go == this.EquipSubMenu)
		{
			return EquipUI.SubMenu.Equip;
		}
		if (go == this.OptimizeSubMenu)
		{
			return EquipUI.SubMenu.Optimize;
		}
		if (go == this.OffSubMenu)
		{
			return EquipUI.SubMenu.Off;
		}
		return EquipUI.SubMenu.None;
	}

	private void Awake()
	{
		base.FadingComponent = this.ScreenFadeGameObject.GetComponent<HonoFading>();
		UIEventListener uieventListener = UIEventListener.Get(this.EquipSubMenu);
		uieventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener.onClick, new UIEventListener.VoidDelegate(this.onClick));
		UIEventListener uieventListener2 = UIEventListener.Get(this.OptimizeSubMenu);
		uieventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener2.onClick, new UIEventListener.VoidDelegate(this.onClick));
		UIEventListener uieventListener3 = UIEventListener.Get(this.OffSubMenu);
		uieventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener3.onClick, new UIEventListener.VoidDelegate(this.onClick));
		this.characterHud = new CharacterDetailHUD(this.CharacterDetailPanel, false);
		this.parameterHud = new EquipUI.ParameterDetailCompareHUD(this.CharacterParameterPanel);
		this.equipmentHud = new EquipmentDetailHud(this.EquipmentPartListPanel.GetChild(0));
		this.weaponPartIconSprite = this.equipmentHud.Weapon.Self.GetChild(2).GetComponent<UISprite>();
		this.weaponPartColonLabel = this.equipmentHud.Weapon.Self.GetChild(3).GetComponent<UILabel>();
		this.equipmentSelectHud = new EquipUI.EquipmentItemListHUD(this.EquipmentAbilityPanel.GetChild(0));
		this.equipmentAbilitySelectHudList[0] = new AbilityItemHUD(this.EquipmentAbilityPanel.GetChild(1).GetChild(0));
		this.equipmentAbilitySelectHudList[1] = new AbilityItemHUD(this.EquipmentAbilityPanel.GetChild(1).GetChild(1));
		this.equipmentAbilitySelectHudList[2] = new AbilityItemHUD(this.EquipmentAbilityPanel.GetChild(1).GetChild(2));
		this.equipmentPartCaption = this.EquipmentPartListPanel.GetChild(1).GetChild(2);
		this.equipmentListCaption = this.EquipmentInventoryListPanel.GetChild(2).GetChild(4).GetChild(0).GetComponent<UILabel>();
		foreach (Object obj in this.EquipmentPartListPanel.GetChild(0).transform)
		{
			Transform transform = (Transform)obj;
			UIEventListener uieventListener4 = UIEventListener.Get(transform.gameObject);
			uieventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uieventListener4.onClick, new UIEventListener.VoidDelegate(this.onClick));
		}
		this.submenuArrowGameObject = this.SubMenuPanel.GetChild(0);
		this.equipSelectScrollList = this.EquipmentInventoryListPanel.GetChild(1).GetComponent<RecycleListPopulator>();
		this.equipmentSelectionTransition = this.TransitionGroup.GetChild(0).GetComponent<HonoTweenPosition>();
		this.avatarTransition = this.CharacterDetailPanel.GetChild(0).GetChild(6).GetChild(0).GetComponent<HonoAvatarTweenPosition>();
		this.itemIdList.Add(new List<FF9ITEM>());
		this.itemIdList.Add(new List<FF9ITEM>());
		this.itemIdList.Add(new List<FF9ITEM>());
		this.itemIdList.Add(new List<FF9ITEM>());
		this.itemIdList.Add(new List<FF9ITEM>());
        this.tempItemList = new FF9ITEM[45];
        for (int i = 0; i < (int)this.tempItemList.Length; i++)
        {
            this.tempItemList[i] = new FF9ITEM(byte.MaxValue, 0);
        }
        this.selectedCaption = this.EquipmentInventoryListPanel.GetChild(2).GetChild(4).GetChild(0).GetComponent<UILocalize>();
		if (FF9StateSystem.MobilePlatform)
		{
			this.EquipSubMenu.GetComponent<ButtonGroupState>().Help.TextKey = "EquipDetailHelpForMobile";
			this.OptimizeSubMenu.GetComponent<ButtonGroupState>().Help.TextKey = "OptimizeHelpForMobile";
			this.OffSubMenu.GetComponent<ButtonGroupState>().Help.TextKey = "OffEquipmentHelpForMobile";
		}
	}

	public GameObject EquipSubMenu;

	public GameObject OptimizeSubMenu;

	public GameObject OffSubMenu;

	public GameObject HelpDespLabelGameObject;

	public GameObject SubMenuPanel;

	public GameObject TransitionGroup;

	public GameObject CharacterDetailPanel;

	public GameObject CharacterParameterPanel;

	public GameObject EquipmentAbilityPanel;

	public GameObject CharacterArrowPanel;

	public GameObject EquipmentPartListPanel;

	public GameObject EquipmentInventoryListPanel;

	public GameObject ScreenFadeGameObject;

	private UISprite weaponPartIconSprite;

	private UILabel weaponPartColonLabel;

	private UILabel equipmentListCaption;

	private CharacterDetailHUD characterHud;

	private EquipUI.ParameterDetailCompareHUD parameterHud;

	private EquipmentDetailHud equipmentHud;

	private GameObject equipmentPartCaption;

	private EquipUI.EquipmentItemListHUD equipmentSelectHud;

	private AbilityItemHUD[] equipmentAbilitySelectHudList = new AbilityItemHUD[3];

	private HonoTweenPosition equipmentSelectionTransition;

	private HonoAvatarTweenPosition avatarTransition;

	private GameObject submenuArrowGameObject;

	private RecycleListPopulator equipSelectScrollList;

	private static String SubMenuGroupButton = "Equip.SubMenu";

	private static String EquipmentGroupButton = "Equip.Equipment";

	private static String InventoryGroupButton = "Equip.Inventory";

	private List<List<FF9ITEM>> itemIdList = new List<List<FF9ITEM>>();

    private FF9ITEM[] tempItemList;

    private Int32 currentPartyIndex;

	private EquipUI.SubMenu currentMenu = EquipUI.SubMenu.None;

	private Int32 currentEquipPart = -1;

	private Int32 currentItemIndex = -1;

	private Boolean fastSwitch;

	private UILocalize selectedCaption;

	private UInt16[] charMask = new UInt16[]
	{
		2048,
		1024,
		512,
		256,
		128,
		64,
		32,
		16,
		8,
		4,
		2,
		1
	};

	private Byte[] partMask = new Byte[]
	{
		128,
		32,
		64,
		16,
		8
	};

	public class ParameterDetailCompareHUD
	{
		public ParameterDetailCompareHUD(GameObject go)
		{
			this.Self = go;
			Int32 i = 0;
			Int32 num = 0;
			while (i < 10)
			{
				if (i != 4)
				{
					this.ParameterLabel[num] = go.GetChild(i).GetChild(1).GetComponent<UILabel>();
					this.NewParameterLabel[num] = go.GetChild(i).GetChild(3).GetComponent<UILabel>();
					this.ArrowSprite[num] = go.GetChild(i).GetChild(2).GetComponent<UISprite>();
					this.ArrowTween[num] = go.GetChild(i).GetChild(2).GetComponent<TweenAlpha>();
					num++;
				}
				i++;
			}
		}

		public GameObject Self;

		public UILabel[] ParameterLabel = new UILabel[9];

		public UILabel[] NewParameterLabel = new UILabel[9];

		public UISprite[] ArrowSprite = new UISprite[9];

		public TweenAlpha[] ArrowTween = new TweenAlpha[9];
	}

	public class EquipmentItemListHUD
	{
		public EquipmentItemListHUD(GameObject go)
		{
			this.Self = go;
			this.PartIconSprite = go.GetChild(0).GetComponent<UISprite>();
			this.Equipment = new ItemListDetailWithIconHUD();
			this.Equipment.IconSprite = go.GetChild(2).GetComponent<UISprite>();
			this.Equipment.NameLabel = go.GetChild(3).GetComponent<UILabel>();
		}

		public GameObject Self;

		public UISprite PartIconSprite;

		public ItemListDetailWithIconHUD Equipment;
	}

	public enum SubMenu
	{
		Equip,
		Optimize,
		Off,
		None
	}

	public class EquipInventoryListData : ListDataTypeBase
	{
		public FF9ITEM ItemData;
	}
}
