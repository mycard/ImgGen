YGOPro卡图生成工具
------------------------------------------------
预览：
https://github.com/mycard/ImgGen/wiki/效果预览

使用：
放到游戏主目录Gen目录，即执行目录上级有cards.cdb
也可以使用本项目的 Dockerfile 构建的镜像。挂载相应数据卷之后直接运行即可。数据卷如下。
/usr/src/app/cards.cdb cdb文件
/usr/src/app/pico 中间图文件
/usr/src/app/ImgGen.exe.config 配置文件，可选。默认为只生成小图，使用隶书字体，黑白
/usr/src/app/pics 生成文件，若生成大图则为 /usr/src/app/picn

必备字体：
文泉驿微米黑
方正隶变_GBK
MatrixBoldSmallCaps

生成卡图：
在执行目录创建pico目录，放入对应密码的中间图规格的png或jpg图片，运行ImgGen.exe，即可在picn目录内生成卡图。

普通中间图尺寸：304x304
灵摆中间图尺寸：347x259

参数：

自定义数据库
ImgGen.exe ..\expansions\pre-release.cdb

配置：

Style
支持隶书和黑体，默认为黑体
NameStyle
支持金色（UR罕贵）和黑白（N罕贵），默认为金色
XyzString
自定义Xyz的翻译，默认为超量
ZeroStarCards
红龙等没有星星的怪兽卡，用逗号分开
GenerateLarge
生成大图，默认为True
GenerateSmall
生成小图，默认为False
GenerateThumb
生成缩略图，默认为False
