# OpenJudger
![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)
![.NET Core](https://img.shields.io/badge/.netcore-2.1-brightgreen.svg)
![Build Status](https://travis-ci.com/Azure99/OpenJudger.svg?branch=master)

## Introduction
OpenJudger is a universal program judger designed to simplify Online Judge System development.<br>
Support [SDNUOJ](https://github.com/sdnuacmicpc/sdnuoj) and [HUSTOJ](https://github.com/zhblue/hustoj).<br>
It is now used to judge solutions on the [SDNUOJ](http://www.acmicpc.sdnu.edu.cn/), [itoIDbOJ](http://db.itoi.sd.cn/) and [itoIOJ](http://oj.itoi.sd.cn/).


## Overview
* Based on .Net Core: cross platform.
* Lightweight: easy to deploy.
* Configurable: all settings are in the Config.json file.
* Multiple languages support: `C`, `C++`, `Java`, `Python`, `Kotlin`, `C#`, `Go`, `NodeJS` and any language that I/O in console.
* Database judge: Mysql only, Support CRUD
* Adaptable: OpenJudger can load your `Fetcher`. Implement `Fetcher` interface to adapt any OnlineJudge System.
* High performance: reliable concurrency control, auto distribute processor affinity.

## Documents
无可奉告(coming soon)

## License
[MIT](http://opensource.org/licenses/MIT)
