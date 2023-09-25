using BattleBitAPI.Common;
using GNA.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GNA.Core.CommandSystem
{
    public class PermissionValidator
    {
        public static async Task<bool> HasPermission(CustomServer server, CustomPlayer player, DefaultGroup needs)
        {
            bool access = false;

            if (needs == DefaultGroup.Everyone)
                return true;

            switch (needs)
            {
                case DefaultGroup.Owner:
                case DefaultGroup.Admin:
                    access = player.StaffRole == Roles.Admin;
                    break;
                case DefaultGroup.Moderator:
                    access = player.StaffRole == Roles.Moderator || player.StaffRole == Roles.Admin;
                    break;
                default:
                    access = true;
                    break;
            }

            return access;
        }
    }
}
