YGOPro卡图生成工具
------------------------------------------------
使用：
放到游戏主目录Gen目录，即执行目录上级有cards.cdb

必备字体：
文泉驿微米黑

生成卡图：
在执行目录创建pico目录，放入对应密码的中间图规格的jpg图片，运行ImgGen.exe，即可在picn目录内生成卡图。

普通中间图尺寸：304x304
灵摆中间图尺寸：347x260

参数：

自定义数据库
ImgGen.exe ..\expansions\pre-release.cdb

配置：

XyzString
自定义Xyz的翻译，默认为超量
FontName
自定义字体，默认为文泉驿微米黑
ZeroStarCards
红龙等没有星星的怪兽卡，用逗号分开
GenerateLarge
生成大图，默认为True
GenerateSmall
生成小图，默认为False
GenerateThumb
生成缩略图，默认为False
