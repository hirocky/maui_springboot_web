using MauiApp1.Application.DTOs.Habits;
using MauiApp1.Application.Services;
using MauiApp1.Domain.Entities;
using MauiApp1.Domain.Repositories;
using Moq;
using Xunit;

namespace MauiApp1.UnitTests.Application.Services;

/// <summary>
/// <see cref="HabitService"/> の単体テスト。
/// リポジトリは Moq でモックし、サービスのロジックのみを検証する。
/// </summary>
public class HabitServiceTests
{
    private readonly Mock<IHabitRepository> _habitRepo;
    private readonly Mock<ICheckInRepository> _checkInRepo;
    private readonly Mock<ICategoryRepository> _categoryRepo;
    private readonly HabitService _sut;

    public HabitServiceTests()
    {
        _habitRepo = new Mock<IHabitRepository>();
        _checkInRepo = new Mock<ICheckInRepository>();
        _categoryRepo = new Mock<ICategoryRepository>();
        _sut = new HabitService(_habitRepo.Object, _checkInRepo.Object, _categoryRepo.Object);
    }

    [Fact]
    public async Task GetTodayTasksAsync_習慣が2件で1件だけチェックイン済み_今日タスクが2件でIsCheckedTodayが正しく付く()
    {
        var today = new DateTime(2025, 3, 14);
        var habits = new List<Habit>
        {
            new() { Id = 1, Name = "朝のストレッチ", TargetFrequencyPerWeek = 7 },
            new() { Id = 2, Name = "読書30分", TargetFrequencyPerWeek = 3 }
        };
        var checkedInIds = new HashSet<int> { 1 };

        _habitRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(habits);
        _checkInRepo.Setup(r => r.GetCheckedInHabitIdsForDateAsync(today)).ReturnsAsync(checkedInIds);

        var result = await _sut.GetTodayTasksAsync(today);

        Assert.Equal(2, result.Count);
        Assert.True(result[0].IsCheckedToday);
        Assert.False(result[1].IsCheckedToday);
        Assert.Equal(1, result[0].Habit.Id);
        Assert.Equal(2, result[1].Habit.Id);
    }

    [Fact]
    public async Task GetTodayTasksAsync_習慣が0件_空リストを返す()
    {
        var today = new DateTime(2025, 3, 14);
        _habitRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Habit>());
        _checkInRepo.Setup(r => r.GetCheckedInHabitIdsForDateAsync(today)).ReturnsAsync(new HashSet<int>());

        var result = await _sut.GetTodayTasksAsync(today);

        Assert.Empty(result);
    }

    [Fact]
    public async Task AddHabitAsync_名前が空_ArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.AddHabitAsync("", 7, "#4CAF50", 0));
    }

    [Fact]
    public async Task AddHabitAsync_正常系_リポジトリのAddAsyncが1回呼ばれ返り値を返す()
    {
        var added = new Habit
        {
            Id = 99,
            Name = "夜の瞑想",
            TargetFrequencyPerWeek = 7,
            ColorHex = "#6200EE",
            CategoryId = 1,
            CreatedAt = DateTime.Now
        };
        _habitRepo.Setup(r => r.AddAsync(It.IsAny<Habit>())).ReturnsAsync(added);

        var result = await _sut.AddHabitAsync("夜の瞑想", 7, "#6200EE", 1);

        Assert.Equal(99, result.Id);
        Assert.Equal("夜の瞑想", result.Name);
        _habitRepo.Verify(r => r.AddAsync(It.Is<Habit>(h => h.Name == "夜の瞑想" && h.TargetFrequencyPerWeek == 7)), Times.Once);
    }
}
