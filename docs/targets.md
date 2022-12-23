:::
:::
## Clean
Cleans bin, obj and artifacts directories
```console
nsn-build clean
```
:::
:::
## Restore
Executes dotnet restore
```console
nsn-build restore
```
:::
:::
## Compile
Executes dotnet build
```console
nsn-build compile
```
:::
:::
## Pack
Creates nuget packages by executing dotnet pack
```console
nsn-build pack
```
:::
:::
## Test
Executes unit tests and generates coverage report
Can get -TestsFilter parameter to filter tests which should be run
```console
nsn-build Test
nsn-build Test -TestsFilter "Category!=IntegrationTest"
```
:::
:::
## PublishPackages
Publishes nuget packages in ./artifacts directory with -Source and -ApiKey parameters.
-Source is "https://api.nuget.org/v3/index.json" by default
```console
nsn-build PublishPackages -ApiKey %SomeApiKey%
```
:::
:::
## QuickRelease
Creates a release branch from dev. Merges it into master. Increments version in dev branch and removes release/* branch.
```console
nsn-build QuickRelease
nsn-build QuickRelease -Force
```
:::
:::
## Publish
Executes dotnet publish
```console
nsn-build publish
```

:::
:::
## Compress
Compresses an artifact to the archive and filters excess files
```console
nsn-build compress
```

:::
:::
## Release
Creates the github release
Gets parameters: GitHubUser, GitHubToken, ReleaseBranch
```console
nsn-build release -GitHubUser Username -GitHubToken %token% 
```

:::
:::
## DockerLogin
Executes "docker login"
Gets parameters: DockerRegistryUrl, DockerUsername, DockerPassword
```console
nsn-build dockerlogin -DockerRegistryUrl https://myregistry.com -DockerUsername user -DockerPassword 12345
```
:::
:::
## BuildImage
Builds docker image
Gets parameters: DockerfilePath, DockerImageFullName
```console
nsn-build buildimage -DockerfilePath ./dockerfile -DockerImageFullName myimage:dev
```
:::
:::
## PushImage
Pushes docker image to the remote registry
Gets parameters: DockerImageFullName
```console
nsn-build PushImage -DockerImageFullName myimage:dev
```
:::
:::
## BuildAndPush
Builds and pushes docker image
Gets parameters: DockerRegistryUrl, DockerUsername, DockerPassword, DockerfilePath, DockerImageFullName
```console
nsn-build BuildAndPush -DockerRegistryUrl https://myregistry.com -DockerUsername user -DockerPassword 12345 -DockerfilePath ./dockerfile -DockerImageFullName myimage:dev
```
:::
