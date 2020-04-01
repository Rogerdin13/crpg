using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces.Events;
using Crpg.Application.Games.Commands;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Games
{
    public class UpsertGameCharacterCommandTest : TestBase
    {
        private static readonly IEventRaiser EventRaiser = Mock.Of<IEventRaiser>();

        [SetUp]
        public Task SetUp()
        {
            var allDefaultItemMbIds = UpsertGameCharacterCommand.Handler.DefaultCharacterItems
                .SelectMany(i => new[]
                {
                    i.HeadItemMbId,
                    i.BodyItemMbId,
                    i.LegItemMbId,
                    i.Weapon1ItemMbId,
                    i.Weapon2ItemMbId,
                })
                .Distinct()
                .Select(mbId => new Item { MbId = mbId });

            _db.Items.AddRange(allDefaultItemMbIds);
            return _db.SaveChangesAsync();
        }

        [Test]
        public async Task UserAndCharacterWithNoEquipmentExist()
        {
            var character = _db.Characters.Add(new Character
            {
                Name = "toto",
                Experience = 100,
                Level = 1,
            });
            var user = _db.Users.Add(new User
            {
                SteamId = 123,
                Characters = new List<Character> { character.Entity },
            });
            await _db.SaveChangesAsync();

            var gc = await new UpsertGameCharacterCommand.Handler(_db, _mapper, EventRaiser).Handle(new UpsertGameCharacterCommand
            {
                SteamId = user.Entity.SteamId,
                CharacterName = character.Entity.Name,
            }, CancellationToken.None);

            Assert.NotNull(gc);
            Assert.AreEqual(user.Entity.Id, gc.UserId);
            Assert.AreEqual(character.Entity.Name, gc.Name);
            Assert.AreEqual(character.Entity.Experience, gc.Experience);
            Assert.AreEqual(character.Entity.Level, gc.Level);
        }

        [Test]
        public async Task UserAndCharacterWithEquipmentExist()
        {
            var character = _db.Characters.Add(new Character
            {
                Name = "toto",
                Experience = 100,
                Level = 1,
                HeadItem = new Item { MbId = "head" },
                CapeItem = new Item { MbId = "cape" },
                BodyItem = new Item { MbId = "body" },
                HandItem = new Item { MbId = "hand" },
                LegItem = new Item { MbId = "leg" },
                HorseHarnessItem = new Item { MbId = "horseHarness" },
                HorseItem = new Item { MbId = "horse" },
                Weapon1Item = new Item { MbId = "weapon1" },
                Weapon2Item = new Item { MbId = "weapon2" },
                Weapon3Item = new Item { MbId = "weapon3" },
                Weapon4Item = new Item { MbId = "weapon4" },
            });
            var user = _db.Users.Add(new User
            {
                SteamId = 123,
                Characters = new List<Character> { character.Entity },
            });
            await _db.SaveChangesAsync();

            var gc = await new UpsertGameCharacterCommand.Handler(_db, _mapper, EventRaiser).Handle(new UpsertGameCharacterCommand
            {
                SteamId = user.Entity.SteamId,
                CharacterName = character.Entity.Name,
            }, CancellationToken.None);

            Assert.NotNull(gc);
            Assert.AreEqual(user.Entity.Id, gc.UserId);
            Assert.AreEqual(character.Entity.Name, gc.Name);
            Assert.AreEqual(character.Entity.Experience, gc.Experience);
            Assert.AreEqual(character.Entity.Level, gc.Level);
            Assert.AreEqual(character.Entity.HeadItem.MbId, gc.HeadItemMbId);
            Assert.AreEqual(character.Entity.CapeItem.MbId, gc.CapeItemMbId);
            Assert.AreEqual(character.Entity.BodyItem.MbId, gc.BodyItemMbId);
            Assert.AreEqual(character.Entity.HandItem.MbId, gc.HandItemMbId);
            Assert.AreEqual(character.Entity.LegItem.MbId, gc.LegItemMbId);
            Assert.AreEqual(character.Entity.HorseHarnessItem.MbId, gc.HorseHarnessItemMbId);
            Assert.AreEqual(character.Entity.HorseItem.MbId, gc.HorseItemMbId);
            Assert.AreEqual(character.Entity.Weapon1Item.MbId, gc.Weapon1ItemMbId);
            Assert.AreEqual(character.Entity.Weapon2Item.MbId, gc.Weapon2ItemMbId);
            Assert.AreEqual(character.Entity.Weapon3Item.MbId, gc.Weapon3ItemMbId);
            Assert.AreEqual(character.Entity.Weapon4Item.MbId, gc.Weapon4ItemMbId);
        }

        [Test]
        public async Task ShouldCreateCharacterIfDoesntExist()
        {
            var user = _db.Users.Add(new User { SteamId = 123 });
            await _db.SaveChangesAsync();

            var gc = await new UpsertGameCharacterCommand.Handler(_db, _mapper, EventRaiser).Handle(new UpsertGameCharacterCommand
            {
                SteamId = user.Entity.SteamId,
                CharacterName = "toto",
            }, CancellationToken.None);

            Assert.NotNull(gc);
            Assert.AreEqual(user.Entity.Id, gc.UserId);
            Assert.AreEqual("toto", gc.Name);
            Assert.AreEqual(0, gc.Experience);
            Assert.AreEqual(1, gc.Level);
            Assert.NotNull(gc.HeadItemMbId);
            Assert.NotNull(gc.BodyItemMbId);
            Assert.NotNull(gc.LegItemMbId);
            Assert.NotNull(gc.Weapon1ItemMbId);
            Assert.NotNull(gc.Weapon2ItemMbId);

            Assert.DoesNotThrowAsync( () => _db.Characters.FirstAsync(c => c.UserId == user.Entity.Id && c.Name == "toto"));
        }

        [Test]
        public async Task ShouldCreateUserAndCharacterIfUserDoesntExist()
        {
            var gc = await new UpsertGameCharacterCommand.Handler(_db, _mapper, EventRaiser).Handle(new UpsertGameCharacterCommand
            {
                SteamId = 123,
                CharacterName = "toto",
            }, CancellationToken.None);

            Assert.NotNull(gc);
            Assert.AreEqual("toto", gc.Name);
            Assert.AreEqual(0, gc.Experience);
            Assert.AreEqual(1, gc.Level);
            Assert.NotNull(gc.HeadItemMbId);
            Assert.NotNull(gc.BodyItemMbId);
            Assert.NotNull(gc.LegItemMbId);
            Assert.NotNull(gc.Weapon1ItemMbId);
            Assert.NotNull(gc.Weapon2ItemMbId);

            Assert.DoesNotThrowAsync(() => _db.Users.FirstAsync(u => u.SteamId == 123));
            Assert.DoesNotThrowAsync(() => _db.Characters.FirstAsync(c => c.UserId == gc.UserId && c.Name == "toto"));
        }
    }
}