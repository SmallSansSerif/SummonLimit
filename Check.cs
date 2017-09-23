using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Terraria;
using TShockAPI;
using static Terraria.ID.ProjectileID;

namespace SummonLimit
{
  public partial class SummonLimit
  {
    /// <summary>
    ///   <see cref="Terraria.ID.ProjectileID" />s of
    ///   known summon minions.
    /// </summary>
    private static readonly int[] Summons =
    {
      Pygmy, Pygmy2, Pygmy3, Pygmy4,
      BabySlime, Hornet, FlyingImp, Retanimini, Spazmamini,
      VenomSpider, JumperSpider, DangerousSpider,
      OneEyedPirate, SoulscourgePirate, PirateCaptain,
      UFOMinion, Raven, Tempest, DeadlySphere,
      StardustCellMinion, StardustDragon2 // including other kinds seem to overcount
    };

    /// <summary>
    ///   <see cref="Terraria.ID.ProjectileID" />s of
    ///   known sentry minions.
    /// </summary>
    private static readonly int[] Sentries =
    {
      SpiderHiver, MoonlordTurret, // queen spider staff & lunar portal staff
      FrostHydra, RainbowCrystal,
      DD2BallistraTowerT1, DD2BallistraTowerT2, DD2BallistraTowerT3,
      DD2LightningAuraT1, DD2LightningAuraT2, DD2LightningAuraT3,
      DD2FlameBurstTowerT1, DD2FlameBurstTowerT2, DD2FlameBurstTowerT3,
      DD2ExplosiveTrapT1, DD2ExplosiveTrapT2, DD2ExplosiveTrapT3
    };

    /// <summary>
    ///   Checks if the given <see cref="Projectile" />
    ///   is a summon minion.
    /// </summary>
    /// <param name="p"><see cref="Projectile" /> to check.</param>
    /// <returns>True if it's a summon projectile, otherwise false.</returns>
    private static bool IsMinion(Projectile p)
    {
      return Summons.Contains(p?.type ?? 0);
    }

    /// <summary>
    ///   Runs every elapse of <see cref="Metronome" />,
    ///   checks current summon projectiles and executes
    ///   <see cref="WarnOrKick" /> where appropriate.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void Check(object sender, ElapsedEventArgs e)
    {
      var players = new Dictionary<TSPlayer, ushort>();

      foreach (
        var projectile in Main.projectile.Where(p => p != null &&
                                                     p.active &&
                                                     IsMinion(p)))
      {
        var player = TShock.Players[projectile.owner];

        if (player == null || !player.Active)
          continue;

        // TODO: Check for rogue retinamini/spazmamini and abusing of stardust dragon heads/tails
        var val =
          (ushort) (projectile.type == 387 || projectile.type == 388 ? 1 : 2);

        if (!players.ContainsKey(player))
          players.Add(player, val);
        else
          players[player] += val;
      }

      foreach (var player in players)
        if (!CheckPermission(player.Key, player.Value))
          WarnOrKick(player.Key);

      players.Clear();
    }

    /// <summary>
    ///   Warns or kicks the player based on their
    ///   status in <see cref="Warned" />.
    /// </summary>
    /// <param name="player">TSPlayer to warn or kick.</param>
    private static void WarnOrKick(TSPlayer player)
    {
      var max = player.Group.GetDynamicPermission(Permission);

      if (!IsWarned(player))
      {
        TShock.Log.Info($"{player.Name} was warned for exceeding minion limit of {max} minions.");
        Warn(player);
      }
      else
      {
        TShock.Log.Info($"{player.Name} was kicked for repeatedly exceeding minion limit of {max} minions.");
        TShock.Utils.Kick(player, $"{KickMessage} ({max})", true);
      }
    }

    /// <summary>
    ///   Checks if the given <see cref="Group" /> is allowed to
    ///   contain given amount of minions.
    /// </summary>
    /// <param name="player">TSPlayer to check.</param>
    /// <param name="amount">Amount of minion projectiles.</param>
    /// <returns></returns>
    private static bool CheckPermission(TSPlayer player, ushort amount)
    {
      var max = player.Group.GetDynamicPermission(Permission) * 2;

      if (max == short.MaxValue * 2)
        return true;

      return amount <= max;
    }
  }
}