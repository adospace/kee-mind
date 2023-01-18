using FluentAssertions;
using KeeMind.Services;
using KeeMind.Services.Data;
using Microsoft.EntityFrameworkCore;

namespace KeeMind.Tests;

[TestClass]
public class DatabaseTests
{
    public TestContext? TestContext { get; set; }
    IServiceProvider _serviceProvider = null!;

    [TestInitialize]
    public void TestInit()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddKeeMindServices();
        serviceCollection.AddKeeMindDummyServices(TestContext.ThrowIfNull().TestName.ThrowIfNull());
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [TestCleanup()]
    public void TestCleanup()
    {
        var repository = _serviceProvider.GetRequiredService<IRepository>();
        repository.DeleteArchive();
    }

    [TestMethod]
    public async Task Create_Local_Archive_With_Password()
    {
        var repository = _serviceProvider.GetRequiredService<IRepository>();

        await repository.CreateArchive("1111");

        using var db = await repository.TryOpenArchive("2222");

        db.Should().BeNull();
    }

    [TestMethod]
    public async Task Should_Be_Able_To_Create_Update_And_Delete_Cards_Creating_a_new_repository()
    {
        int cardId;

        {
            var repository = _serviceProvider.GetRequiredService<IRepository>();

            await repository.CreateArchive("1111");
        }

        {
            var repository = _serviceProvider.GetRequiredService<IRepository>();
            await using var db = repository.OpenArchive();

            var newCard = new Card
            {
                Name = "Test"
            };

            db.Cards.Add(newCard);

            await db.SaveChangesAsync();

            cardId = newCard.Id;
        }

        {
            var repository = _serviceProvider.GetRequiredService<IRepository>();
            await using var db = repository.OpenArchive();
            var createdCard = await db.Cards.FirstAsync(_=>_.Id == 1);

            createdCard.Name.Should().Be("Test");

            createdCard.Name = "changed";

            await db.SaveChangesAsync();
        }

        {
            var repository = _serviceProvider.GetRequiredService<IRepository>();
            await using var db = repository.OpenArchive();

            var updatedCard = await db.Cards.FirstAsync(_ => _.Id == 1);

            updatedCard.Name.Should().Be("changed");

            db.Cards.Remove(updatedCard);

            await db.SaveChangesAsync();
        }
        {
            var repository = _serviceProvider.GetRequiredService<IRepository>();
            await using var db = repository.OpenArchive();
            var deletedCard = await db.Cards.FirstOrDefaultAsync(_ => _.Id == 1);

            deletedCard.Should().BeNull();

        }
    }

    [TestMethod]
    public async Task Should_Be_Able_To_Create_Update_And_Delete_Cards_Reusing_the_repository()
    {
        int cardId;
        var repository = _serviceProvider.GetRequiredService<IRepository>();

        await repository.CreateArchive("1111");
        await using var db = repository.OpenArchive();

        Card newCard = new Card
        {
            Name = "Test"
        };

        db.Cards.Add(newCard);

        await db.SaveChangesAsync();

        cardId = newCard.Id;

        var createdCard = await db.Cards.FirstAsync(_=>_.Id == cardId);

        createdCard.Name.Should().Be("Test");

        createdCard.Name = "changed";

        await db.SaveChangesAsync();

        var updatedCard = await db.Cards.FirstAsync(_ => _.Id == cardId);

        updatedCard.Should().NotBeNull();
        updatedCard.Name.Should().Be("changed");

        db.Cards.Remove(updatedCard);

        await db.SaveChangesAsync();

        var deletedCard = await db.Cards.FirstOrDefaultAsync(_ => _.Id == cardId);

        deletedCard.Should().BeNull();
    }

    [TestMethod]
    public async Task Should_Be_Able_To_Create_Update_And_Delete_Cards_With_Items()
    {
        int cardId;

        var repository = _serviceProvider.GetRequiredService<IRepository>();

        await repository.CreateArchive("1111");
        await using var db = repository.OpenArchive();

        var newCard = new Card
        {
            Name = "Test"
        };
        
        newCard.Items.Add(new Item
        {
            Card = newCard,
            Label = "TestLabel",
            Value = "TestValue",
            Type = ItemType.Email
        });

        db.Cards.Add(newCard);

        await db.SaveChangesAsync();

        cardId = newCard.Id;

        var createdCard = await db.Cards.FirstAsync(_ => _.Id == cardId);

        createdCard.Should().NotBeNull();
        createdCard.Name.Should().Be("Test");

        createdCard.Name = "changed";


        await db.SaveChangesAsync();

        var updatedCard = await db.Cards.FirstAsync(_ => _.Id == cardId);

        updatedCard.Should().NotBeNull();
        updatedCard.Name.Should().Be("changed");

        updatedCard.Items[0].Label.Should().Be("TestLabel");
        updatedCard.Items[0].Value.Should().Be("TestValue");

        db.Cards.Remove(updatedCard);

        await db.SaveChangesAsync();

        var deletedCard = await db.Cards.FirstOrDefaultAsync(_ => _.Id == cardId);

        deletedCard.Should().BeNull();

    }

    [TestMethod]
    public async Task Should_Be_Able_To_Create_Update_And_Delete_Items_Insede_A_Card()
    {
        int cardId;
        var repository = _serviceProvider.GetRequiredService<IRepository>();

        await repository.CreateArchive("1111");
        await using var db = repository.OpenArchive();

        var newCard = new Card
        {
            Name = "Test"
        };

        newCard.Items.Add(new Item
        {
            Card = newCard,
            Label = "item1",
            Value = "item1_value",
            Type = ItemType.Email
        });

        newCard.Items.Add(new Item
        {
            Card = newCard,
            Label = "item2",
            Value = "item2_value",
            Type = ItemType.PIN
        });

        db.Cards.Add(newCard);

        await db.SaveChangesAsync();
        
        cardId = newCard.Id;

        newCard.Items[0].Label.Should().Be("item1");
        newCard.Items[1].Label.Should().Be("item2");
        newCard.Items[0].Value.Should().Be("item1_value");
        newCard.Items[1].Value.Should().Be("item2_value");

        var createdCard = await db.Cards.FirstAsync(_ => _.Id == cardId);

        createdCard.Items[0].Label.Should().Be("item1");
        createdCard.Items[1].Label.Should().Be("item2");
        createdCard.Items[0].Value.Should().Be("item1_value");
        createdCard.Items[1].Value.Should().Be("item2_value");

        createdCard.Items[0].Value = "item1_value_changed";

        await db.SaveChangesAsync();

        createdCard = await db.Cards.FirstAsync(_ => _.Id == cardId);

        createdCard.Items.Count.Should().Be(2);
        createdCard.Items[0].Label.Should().Be("item1");
        createdCard.Items[1].Label.Should().Be("item2");
        createdCard.Items[0].Value.Should().Be("item1_value_changed");
        createdCard.Items[1].Value.Should().Be("item2_value");

        createdCard.Items.RemoveAt(0);

        createdCard.Items.Count.Should().Be(1);
        createdCard.Items[0].Label.Should().Be("item2");
        createdCard.Items[0].Value.Should().Be("item2_value");

        createdCard.Items.Add(new Item
        {
            Card = createdCard,
            Label = "item3",
            Value = "item3 value",
        });

        await db.SaveChangesAsync();

        createdCard = await db.Cards.FirstAsync(_ => _.Id == cardId);

        createdCard.Items.Count.Should().Be(2);
        createdCard.Items[0].Label.Should().Be("item2");
        createdCard.Items[1].Label.Should().Be("item3");
        createdCard.Items[0].Value.Should().Be("item2_value");
        createdCard.Items[1].Value.Should().Be("item3 value");

        db.Cards.Remove(createdCard);

        await db.SaveChangesAsync();

        var deletedCard = await db.Cards.FirstOrDefaultAsync(_ => _.Id == cardId);

        deletedCard.Should().BeNull();

    }

    [TestMethod]
    public async Task Should_Be_Able_To_Create_Update_And_Delete_Tags_Insede_A_Card()
    {
        int cardId;
        Card newCard;
        Tag[] allTags;

        {
            var repository = _serviceProvider.GetRequiredService<IRepository>();

            await repository.CreateArchive("1111");
        }
        {
            var repository = _serviceProvider.GetRequiredService<IRepository>();
            await using var db = repository.OpenArchive();

            newCard = new Card
            {
                Name = "Test"
            };

            var newTag = new Tag()
            {
                Name = "tag1"
            };

            var newTagEntry = new TagEntry() { Card = newCard, Tag = newTag };
            newCard.Tags.Add(newTagEntry);

            newCard.Tags.Count.Should().Be(1);

            //db.Tags.Add(newTag);
            db.Cards.Add(newCard);

            //db.Entry(newTagEntry).State = EntityState.Added;

            await db.SaveChangesAsync();
        }
        {
            var repository = _serviceProvider.GetRequiredService<IRepository>();
            await using var db = repository.OpenArchive();

            cardId = newCard.Id;

            newCard = await db.Cards.Include(_ => _.Tags).ThenInclude(_ => _.Tag).FirstAsync(_ => _.Id == cardId);

            newCard.Tags.Count.Should().Be(1);
            newCard.Tags[0].Tag.Name.Should().Be("tag1");

            allTags = await db.Tags.ToArrayAsync();

            allTags.Should().HaveCount(1);
            allTags[0].Name.Should().Be("tag1");

            var secondTag = new Tag
            {
                Name = "tag2"
            };
        
            newCard.Tags.Add(new TagEntry { Card = newCard, Tag = secondTag });
            newCard.Tags.RemoveAt(0);

            newCard.Tags.Count.Should().Be(1);

            await db.SaveChangesAsync();

        }
        {
            var repository = _serviceProvider.GetRequiredService<IRepository>();
            await using var db = repository.OpenArchive();
            var createdCard = await db.Cards.Include(_ => _.Tags).ThenInclude(_ => _.Tag).FirstAsync(_ => _.Id == cardId);

            createdCard.Tags.Count.Should().Be(1);
            createdCard.Tags[0].Tag.Name.Should().Be("tag2");

            allTags = await db.Tags.ToArrayAsync();

            allTags.Should().HaveCount(2);
            allTags[0].Name.Should().Be("tag1");
            allTags[1].Name.Should().Be("tag2");

            var secondCard = new Card
            {
                Name = "Second test"
            };

            db.Cards.Add(secondCard);

            secondCard.Tags.Add(new TagEntry { Card = secondCard, Tag = allTags[1] });

            await db.SaveChangesAsync();

            allTags = await db.Tags.ToArrayAsync();

            allTags.Should().HaveCount(2);
            allTags[0].Name.Should().Be("tag1");
            allTags[1].Name.Should().Be("tag2");

            secondCard = await db.Cards.FirstAsync(_=>_.Id == secondCard.Id);

            secondCard.Tags.Count.Should().Be(1);
            secondCard.Tags[0].Tag.Name.Should().Be("tag2");


            newCard.Tags.RemoveAt(0);

            await db.SaveChangesAsync();

            allTags = await db.Tags.ToArrayAsync();

            allTags.Should().HaveCount(2);
            allTags[0].Name.Should().Be("tag1");
            allTags[1].Name.Should().Be("tag2");

            db.Cards.Remove(createdCard);

            await db.SaveChangesAsync();

            var deletedCard = await db.FindAsync<Card>(cardId);

            deletedCard.Should().BeNull();
        }

    }


    [TestMethod]
    public async Task Should_Be_Able_To_Create_Update_And_Delete_Tags_Insede_A_Disconnected_Card()
    {
        Card newCard;

        {
            var repository = _serviceProvider.GetRequiredService<IRepository>();

            await repository.CreateArchive("1111");

            await using var db = repository.OpenArchive();

            newCard = new Card
            {
                Name = "Test"
            };

            db.Cards.Add(newCard);

            await db.SaveChangesAsync();
        }

        { 
            var tag = new Tag { Name = "Test" };
            var tagEntry = new TagEntry { Card = newCard, Tag = tag };
            newCard.Tags.Add(tagEntry);

            var repository = _serviceProvider.GetRequiredService<IRepository>();

            await using var db = repository.OpenArchive();

            db.Entry(tagEntry).State = EntityState.Added;
            db.Entry(tag).State = EntityState.Added;

            await db.SaveChangesAsync();
        }
    }

}