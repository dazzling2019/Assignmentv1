namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<Assignmentv1Context>()
            .UseInMemoryDatabase(databaseName: "Assignmentv1sDatabase" + DateTime.Now.Ticks)
            .Options;
            using (var context = new Assignmentv1Context(options))
            {
                context.Room.Add(new Room { Id = 1, Name = "Test Room 1" });
                context.Room.Add(new Room { Id = 2, Name = "Test Room 2" });
                context.Room.Add(new Room { Id = 3, Name = "Test Room 3" });
                context.SaveChanges();
            }
            using (var context = new Assignmentv1Context(options))
            {
                var controller = new RoomsController(context);
                // Act
                var result = await controller.GetRoom();
                // Assert
                // the response should be a ActionResult, is it?
                var actionResult =
                Assert.IsType<ActionResult<IEnumerable<Room>>>(result);
                // the response returned by the ActionResult should contain a List of Room
                //objects, does it?
var list =
Assert.IsType<List<Room>>(actionResult.Value);
                // the List of Genres should contain no more (and no less) than 3 Room
                //objects, does it?
Assert.Equal(3, list.Count);
            }
        }
    }
    }
}