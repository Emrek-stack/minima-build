:::
:::
## Clean
Cleans bin, obj and artifacts directories
```console
mn-build clean
```
:::
:::
## Restore
Executes dotnet restore
```console
mn-build restore
```
:::
:::
## Compile
Executes dotnet build
```console
mn-build compile
```
:::
:::
## Pack
Creates nuget packages by executing dotnet pack
```console
mn-build pack
```
:::
:::
## Test
Executes unit tests and generates coverage report
Can get -TestsFilter parameter to filter tests which should be run
```console
mn-build Test
mn-build Test -TestsFilter "Category!=IntegrationTest"
```
:::
:::
## PublishPackages
Publishes nuget packages in ./artifacts directory with -Source and -ApiKey parameters.
-Source is "https://api.nuget.org/v3/index.json" by default
```console
mn-build PublishPackages -ApiKey %SomeApiKey%
```
:::
:::
## QuickRelease
Creates a release branch from dev. Merges it into master. Increments version in dev branch and removes release/* branch.
```console
mn-build QuickRelease
mn-build QuickRelease -Force
```
:::
:::
## Publish
Executes dotnet publish
```console
mn-build publish
```

:::
:::
## Compress
Compresses an artifact to the archive and filters excess files
```console
mn-build compress
```

:::
:::
## Release
Creates the github release
Gets parameters: GitHubUser, GitHubToken, ReleaseBranch
```console
mn-build release -GitHubUser Username -GitHubToken %token%
```

:::
:::
## DockerLogin
Executes "docker login"
Gets parameters: DockerRegistryUrl, DockerUsername, DockerPassword
```console
mn-build dockerlogin -DockerRegistryUrl https://myregistry.com -DockerUsername user -DockerPassword 12345
```
:::
:::
## BuildImage
Builds docker image
Gets parameters: DockerfilePath, DockerImageFullName
```console
mn-build buildimage -DockerfilePath ./dockerfile -DockerImageFullName myimage:dev
```
:::
:::
## PushImage
Pushes docker image to the remote registry
Gets parameters: DockerImageFullName
```console
mn-build PushImage -DockerImageFullName myimage:dev
```
:::
:::
## BuildAndPush
Builds and pushes docker image
Gets parameters: DockerRegistryUrl, DockerUsername, DockerPassword, DockerfilePath, DockerImageFullName
```console
mn-build BuildAndPush -DockerRegistryUrl https://myregistry.com -DockerUsername user -DockerPassword 12345 -DockerfilePath ./dockerfile -DockerImageFullName myimage:dev
```
:::
