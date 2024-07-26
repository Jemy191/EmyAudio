using ConsoleClient.Pages;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ConsoleClient;

public class PageNavigationService
{
    readonly IServiceProvider serviceProvider;

    IPage? currentPage;
    readonly Stack<IPage> pagesStack = new Stack<IPage>();
    
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
        Refresh();

        var result = await page.ShowAsync();
        
        if (pagesStack.Count != 0)
            currentPage = pagesStack.Pop();
        else
            currentPage = null;
        
        return result;
    }

    public async Task Open(Type pageType)
    {
        if (currentPage is not null)
            pagesStack.Push(currentPage);
        
        var pageInstance = ActivatorUtilities.CreateInstance(serviceProvider, pageType);
        if (pageInstance is not IPage page)
            throw new NullReferenceException($"Page of type {pageType} not found");

        currentPage = page;
        Refresh();

        await page.Show();

        if (pagesStack.Count != 0)
            currentPage = pagesStack.Pop();

        currentPage = null;
    }

    public void Refresh()
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
}