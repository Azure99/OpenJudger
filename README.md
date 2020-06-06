# OpenJudger
![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)
![.NET Core](https://img.shields.io/badge/.netcore-3.1-brightgreen.svg)
![Build Status](https://travis-ci.com/Azure99/OpenJudger.svg?branch=master)

## Introduction
OpenJudger is a lightweight, high performance and universal program judger designed to simplify Online Judge System development.

Support [SDNUOJ](https://github.com/sdnuacmicpc/sdnuoj), [HUSTOJ](https://github.com/zhblue/hustoj) and more!

It is now used to judge solutions on the [itoIDbOJ](http://db.itoi.sd.cn/), [itoIOJ](http://oj.itoi.sd.cn/) and [SDNUOJ](http://www.acmicpc.sdnu.edu.cn/).


## Overview
* Based on .Net Core: cross platform, easy to deploy.
* Lightweight: minimum dependencies (Newtonsoft.Json only).
* Configurable: provide custom options in Config.json file.
* Multiple languages support: `C`, `C++`, `Java`, `Python`, `Kotlin`, `C#`, `Go`, `NodeJS`... Almost any programming language!
* Special judge: Use your program to check user's answer.
* Database judge: MySQL, support Create Read Update Delete.
* Adaptable: provide`Judger.Adapter` interface to adapt your Online Judge System.
* High performance: reliable concurrency control, auto distribute processor affinity.

## Documents (CN)
* [Configuration](https://github.com/Azure99/OpenJudger/wiki/config_zh)

## [Benchmark](https://github.com/Azure99/OpenJudger/wiki/benchmark_zh)
|     Item      | Open Judger | HUSTOJ Judger |
| :-----------: | ----------- | ------------- |
|    Simple     | 43.44s      | 111.84s       |
| I/O intensive | 83.24s      | 107.62s       |
| CPU intensive | 54.38s      | 66.51s        |
|      Sum      | 181.06s     | 285.97s       |

## License
[MIT](http://opensource.org/licenses/MIT)
