# OpenJudger
![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)
![.NET Core](https://img.shields.io/badge/.NET-5.0-brightgreen.svg)
![Build Status](https://travis-ci.com/Azure99/OpenJudger.svg?branch=master)

## Introduction
OpenJudger is a lightweight, high performance, universal program judger designed to simplify Online Judge System development. It's extensible, can extend programing language, database middle layer, server adapter, event handler even new judge type.

Official adapter: [SDNUOJ](https://github.com/sdnuacmicpc/sdnuoj) and [HUSTOJ](https://github.com/zhblue/hustoj)

## Overview
* Based on .Net Core: cross platform, easy to deploy.
* Lightweight: minimum dependencies (Newtonsoft.Json only).
* Configurable: provide many custom options in Config.json file.
* Multiple languages support: `C`, `C++`, `Java`, `Python`, `Kotlin`, `C#`, `Go`, `NodeJS`... Almost any programming language!
* Special judge: Use your program to check user's answer.
* SQL judge: MySQL, support Create Read Update Delete.
* Adaptable: provide`Judger.Adapter` interface to adapt your Online Judge System.
* High performance: reliable concurrency control, excellent task scheduling, auto manage processor affinity.

## Who's using?
* [山东师范大学OnlineJudge](http://www.acmicpc.sdnu.edu.cn/)
* [HUSTOJ By zhblue](https://github.com/zhblue/hustoj/)
* [山东师范大学数据库实训平台](http://db.itoi.sd.cn/)
* 山东师范大学编程考试系统

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
