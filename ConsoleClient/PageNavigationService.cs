using ConsoleClient.Pages;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ConsoleClient;

public class PageNavigationService
{
    readonly IServiceProvider serviceProvider;

    IPage? currentPage;
    readonly Stack<IPage> pagesStack = new Stack<IPage>();

    public bool ExitRequested { get; set; }

    public PageNavigationService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }
    
    public async Task Open<T>() where T : IPage => await Open(typeof(T));

    public async Task<TR> Open<T, TR>() where T : IPage<TR>
    {
        if(currentPage is not null)
            pagesStack.Push(currentPage);
        
        var page = ActivatorUtilities.CreateInstance<T>(serviceProvider);

        currentPage = page;
        RefreshShell();

        var result = await page.ShowAsync();

        NavigateBack();
            return result;
    }

    public async Task Open(Type pageType)
    {
        if (currentPage is not null)
            pagesStack.Push(currentPage);
        
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        
        var pageInstance = ActivatorUtilities.CreateInstance(serviceScope.ServiceProvider, pageType);
        if (pageInstance is not IPage page)
            throw new NullReferenceException($"Page of type {pageType} not found");

        currentPage = page;
        RefreshShell();

        await page.Show();
        
        NavigateBack();
    }
    void NavigateBack()
    {
        if (pagesStack.Count == 0)
        {
            currentPage = null;
            return;
        }

        if (ExitRequested)
            return;
        currentPage = pagesStack.Pop();
        RefreshShell();
    }

    public void RefreshShell()
    {
        if (currentPage == null)
            throw new NullReferenceException("Current page is null");
        
        Shell(currentPage);
    }
    
    static void Shell(IPageInfo page)
    {
        AnsiConsole.Clear();
        
        AnsiConsole.Write(
            new FigletText(page.Name)
                .Centered()
                .Color(Color.White));
    }
    public void RequestExit()
    {
        ExitRequested = true;
    }
}