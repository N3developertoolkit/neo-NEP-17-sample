# Neo N3 Fungible Token Sample Contract

This repo contains a sample Neo N3 blockchain contract implementing the [NEP-17 fungible token standard](https://github.com/neo-project/proposals/pull/126).

## Prerequisites

> Note, if you're using [VS Code Remote Container](https://code.visualstudio.com/docs/remote/containers)
  or [GitHub Codespaces](https://github.com/features/codespaces),
  the [devcontainer Dockerfile](.devcontainer/Dockerfile) for this repo has all the prerequisites installed.

- [.NET 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/5.0)
- [Visual Studio Code (v1.52 or later)](https://code.visualstudio.com/Download)

### Ubuntu Prerequisites

Installing on Ubuntu 18.04 or 20.04 also requires installing libsnappy-dev and libc6-dev
via apt-get. 

``` shell
$ sudo apt install libsnappy-dev libc6-dev -y
```

### MacOS Prerequisites

Installing on MacOS requires installing rocksdb via [Homebrew](https://brew.sh/).

``` shell
$ brew install rocksdb
```

