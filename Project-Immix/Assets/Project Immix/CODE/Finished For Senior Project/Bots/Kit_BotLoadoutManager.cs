﻿using UnityEngine;

namespace ImmixKit
{
    public abstract class Kit_BotLoadoutManager : ScriptableObject
    {
        /// <summary>
        /// Gets a loadout for a bot
        /// </summary>
        /// <param name="main"></param>
        /// <returns></returns>
        public abstract Loadout GetBotLoadout(Kit_IngameMain main);
    }
}
