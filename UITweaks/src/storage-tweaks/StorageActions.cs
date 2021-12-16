using System;
using UnityEngine;
using Common;

#if GAME_BZ
using System.Collections.Generic;
#endif

namespace UITweaks.StorageTweaks
{
	static partial class StorageActions
	{
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
		class StorageHandlerAttribute: Attribute
		{
			readonly TechType techType;
			public string classId => CraftData.GetClassIdForTechType(techType);

			public StorageHandlerAttribute(TechType techType) => this.techType = techType;
		}

		#region abstract base classes
		interface IActionHandler
		{
			string actions { get; }
			void processActions();
		}

		abstract class StorageAction: MonoBehaviour, IActionHandler
		{
			protected StorageContainer container;

			protected string _actions;
			public virtual string actions => _actions;

			protected virtual void Start()
			{
				container = GetComponent<StorageContainer>();
				Common.Debug.assert(container);
			}

			public abstract void processActions();
		}

		abstract class StorageOpenAction: StorageAction
		{
			public override string actions =>
				_actions ??= HandReticle.main.GetText(container.hoverText, true, GameInput.Button.LeftHand);
		}

		abstract class StorageRenameAction: StorageOpenAction
		{
			const GameInput.Button actionButton = GameInput.Button.RightHand;
			protected ColoredLabel label;

			public override string actions =>
				_actions ??= base.actions + (!label? "": $"\n{HandReticle.main.GetText(label.stringEditLabel, true, actionButton)}");

			public override void processActions()
			{
				if (GameInput.GetButtonDown(actionButton))
					label?.OnHandClick(Player.main.GetComponent<GUIHand>());
			}
		}
		#endregion

		#region action handlers
		[StorageHandler(TechType.SmallLocker)]
		class SmallLockerRenameAction: StorageRenameAction
		{
			protected override void Start()
			{
				base.Start();
				label = gameObject.getChild("Label").GetComponent<ColoredLabel>();
			}
		}

#if GAME_SN
		[StorageHandler(TechType.LuggageBag)]
#elif GAME_BZ
		[StorageHandler(TechType.QuantumLocker)]
#endif
		[StorageHandler(TechType.SmallStorage)]
		class StoragePickupAction: StorageRenameAction
		{
			const GameInput.Button actionButton = GameInput.Button.AltTool;
			PickupableStorage storage;

			public override string actions
			{
				get
				{
					if (_actions == null)
					{
						var techType = storage.pickupable.GetTechType();
						string packOrPick = storage.pickupable.usePackUpIcon? LanguageCache.GetPackUpText(techType): LanguageCache.GetPickupText(techType);
						string action = $"\n{HandReticle.main.GetText(packOrPick, false, actionButton)}";

						_actionDisabled = base.actions + $"<color=#888888>{action}</color>"; // '_actions' changes here
						_actions += action;
					}

					bool allowedToPickUp = (container.IsEmpty() || storage.allowPickupWhenNonEmpty) && storage.pickupable.AllowedToPickUp();
					return allowedToPickUp? _actions: _actionDisabled;
				}
			}
			string _actionDisabled;

			protected override void Start()
			{
				base.Start();

				label = gameObject.getParent().GetComponentInChildren<ColoredLabel>();
				storage = gameObject.getParent().GetComponentInChildren<PickupableStorage>(true);
			}

			public override void processActions()
			{
				base.processActions();

				if (!GameInput.GetButtonDown(actionButton))
					return;

				if (!Player.main.HasInventoryRoom(storage.pickupable))
					Language.main.Get("InventoryFull").onScreen();
				else
					storage.OnHandClick(Player.main.GetComponent<GUIHand>());
			}
		}

#if GAME_BZ
		[StorageHandler(TechType.SeaTruckStorageModule)]
		[StorageHandler(TechType.SeaTruckFabricatorModule)]
		class SeaTruckStorageRenameAction: StorageRenameAction
		{
			static readonly Dictionary<string, string> labels = new() // :((
			{
				{ "StorageContainer", "Label (2)" },
				{ "StorageContainer (1)", "Label (4)" },
				{ "StorageContainer (2)", "Label" },
				{ "StorageContainer (3)", "Label (1)" },
				{ "StorageContainer (4)", "Label (3)" }
			};

			protected override void Start()
			{
				base.Start();
				Common.Debug.assert(labels.ContainsKey(gameObject.name), $"name not found: '{gameObject.name}'");

				if (labels.TryGetValue(gameObject.name, out string labelName))
					label = gameObject.getChild($"../{labelName}").GetComponent<ColoredLabel>();
			}
		}
#endif
		#endregion
	}
}