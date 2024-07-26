namespace ConsoleClient.Pages;

public interface IPage : IPageInfo
{
    Task Show();
}

public interface IPage<T> : IPage
{
    Task<T> ShowAsync();

    async Task IPage.Show() => await ShowAsync();
}