YGOPro卡图生成工具
------------------------------------------------
预览：
https://github.com/mycard/ImgGen/wiki/效果预览

使用：
放到游戏主目录Gen目录，即执行目录上级有cards.cdb

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
