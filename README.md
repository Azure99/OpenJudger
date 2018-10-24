# OpenJudger
![GitHub](https://img.shields.io/github/license/mashape/apistatus.svg)
![.NET Core](https://img.shields.io/badge/.netcore-2.1-brightgreen.svg)
![Build Status](https://travis-ci.com/Azure99/OpenJudger.svg?branch=master)

## Introduction
OpenJudger is a universal program judger designed to simplify Online Judge System development.<br>
It is now used to judge solutions on the [SDNUOJ](http://www.acmicpc.sdnu.edu.cn/).

## Overview
* Based on .Net Core: cross platform.
* Lightweight: easy to deploy.
* Configurable: all settings are in the Config.json file, have unusual readability.
* Multiple languages support: `C`, `C++`, `Java`, `Python`, `Kotlin` and any language that I/O in console.
* Adaptable: OpenJudger can load your `Fetcher` by reflection. Implement `Fetcher` interface to adapt any OnlineJudge System.
* High performance: reliable concurrency control, auto distribute processor affinity.

## License
[MIT](http://opensource.org/licenses/MIT)
