using System;
using CustomDynamicListLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using DotnetProject.DAL;
using DotnetProject.PL;

class DotnetPrj
{
    static void Main()
    {
        using (var context = new DotnetProjectDbContext())
        {
            ConsoleInteraction interaction = new ConsoleInteraction(context);

            interaction.ImitateSocialMediaActivity();
        }
    }
}