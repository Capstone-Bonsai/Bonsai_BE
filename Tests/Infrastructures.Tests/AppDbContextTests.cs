using Domain.Tests;

namespace Infrastructures.Tests
{
    public class AppDbContextTests : SetupTest, IDisposable
    {
        /*[Fact]
        public async Task AppDbContext_ChemicalsDbSetShouldReturnCorrectData()
        {

            var mockData = _fixture.Build<Chemical>().CreateMany(10).ToList();
            await _dbContext.Chemicals.AddRangeAsync(mockData);
            
            await _dbContext.SaveChangesAsync();

            var result = await _dbContext.Chemicals.ToListAsync();
            result.Should().BeEquivalentTo(mockData);
        }

        [Fact]
        public async Task AppDbContext_ChemicalsDbSetShouldReturnEmptyListWhenNotHavingData()
        {
            var result = await _dbContext.Chemicals.ToListAsync();
            result.Should().BeEmpty();
        }*/
    }
}
