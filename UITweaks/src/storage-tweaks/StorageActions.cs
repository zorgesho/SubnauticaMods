using System;

using UnityEngine;

using Common;
using Common.Reflection;

namespace UITweaks.StorageTweaks
{
	interface IStorageActions
	{
		string actions { get; }
		void processActions();
	}

	static partial class StorageActions
	{
		// we'll use inheritance for actions
		// it's not quite correct, but it'll simplify the code and good enough in this case
		abstract class StorageAction: MonoBehaviour, IStorageActions
		{
			static readonly EventWrapper onBindingsChanged = typeof(GameInput).evnt("OnBindingsChanged").wrap();

			protected StorageContainer container;

			protected string _actions;
			public virtual string actions => _actions;

			void clearCachedActions() => _actions = null;

			void Awake() => onBindingsChanged.add<Action>(clearCachedActions);
			void OnDestroy() => onBindingsChanged.remove<Action>(clearCachedActions);

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

		[StorageHandler(TechType.SmallLocker)]
#if GAME_BZ
		[StorageHandler(TechType.SeaTruckStorageModule)]
		[StorageHandler(TechType.SeaTruckFabricatorModule)]
#endif
		class StorageRenameAction: StorageOpenAction
		{
			const GameInput.Button actionButton = GameInput.Button.RightHand;

			ColoredLabel label;

			protected override void Start()
			{
				base.Start();
				label = gameObject.GetComponent<IStorageLabel>()?.label;
			}

			public override string actions =>
				_actions ??= base.actions + (!label? "": $"\n{HandReticle.main.GetText(label.stringEditLabel, true, actionButton)}");

			public override void processActions()
			{
				if (GameInput.GetButtonDown(actionButton))
					label?.OnHandClick(Player.main.GetComponent<GUIHand>());
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

					bool allowedToPickUp = (container.IsEmpty() || storage.isAllowedToPickUpNonEmpty()) && storage.pickupable.AllowedToPickUp();
					return allowedToPickUp? _actions: _actionDisabled;
				}
			}
			string _actionDisabled;

			protected override void Start()
			{
				base.Start();
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
	}
}