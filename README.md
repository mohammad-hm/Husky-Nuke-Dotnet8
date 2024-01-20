# Husky-Nuke-Dotnet8
# Husky

1. create new console app project.

2. create sln, beside the scproj file.

3. run this command:

   ```bash
   dotnet new tool-manifest
   dotnet tool install Husky
   ```

- The first command, `dotnet new tool-manifest`, creates a manifest file in your project directory that lists the tools that you want to install locally. The manifest file is named `dotnet-tools.json` and it is stored in a `.config` folder. You can use this file to share the tools with other developers who work on the same project.

- The second command, `dotnet tool install Husky`, installs the Husky tool from the NuGet package and adds it to the manifest file.

4. Setup husky for your project using this command

   ```bash
   dotnet husky install
   ```

5. Attach Husky to your project then This way the other contributors will use your pre-configured tasks automatically using this command:  

   ​	`dotnet husky attach <path-to-project-file>`

6. Finally copy all config from .husky folder from another project to this project.

-------------------------------------------------------------------------------------

# Nuke

1. Install the NUKE global tool by running this command: `dotnet tool install Nuke.GlobalTool --global`
2. Set up a new build project by running this command: `nuke :setup`
3. Choose a name, a target framework, and a solution for your build project.
4. Edit the generated `Build.cs` file to add your build steps and dependencies.
5. Run your build by running this command: `nuke`
   - consider after this step, copy all files from another project and just change name of the project .
