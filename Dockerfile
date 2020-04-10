FROM mono:5.20.1.34 as sqlite-builder

RUN apt-get update && \
	apt-get -y install wget sqlite3 libsqlite3-dev p7zip-full && \
	rm -rf /var/lib/apt/lists/*
WORKDIR /
RUN wget https://minio.mycard.moe:9000/nanahira/sqlite-netFx-full-source-1.0.112.0.zip && \
	7z x -y -osqlite sqlite-netFx-full-source-1.0.112.0.zip && \
	cd sqlite && \
	xbuild /p:Configuration=Release /p:UseInteropDll=false /p:UseSqliteStandard=true ./System.Data.SQLite/System.Data.SQLite.2015.csproj

FROM mono:5.20.1.34 as builder

COPY . /ImgGen
WORKDIR /ImgGen
COPY --from=sqlite-builder /sqlite/bin/2015/ReleaseMonoOnPosix/bin/System.Data.SQLite.dll .
RUN xbuild /p:Configuration=Release /p:TargetFrameworkVersion=v4.6

FROM mono:5.20.1.34

RUN apt-get update && \
	apt-get -y install xfonts-utils fontconfig wget p7zip-full && \
	rm -rf /var/lib/apt/lists/*

WORKDIR /
RUN wget https://minio.mycard.moe:9000/nanahira/ImgGen-Fonts.7z && \
	7z x -y -o/usr/share/fonts ImgGen-Fonts.7z && \
	rm -rf ImgGen-Fonts.7z && \
	mkfontscale && \
	mkfontdir && \
	fc-cache -fv

COPY --from=builder /ImgGen/bin/Release /usr/src/app
WORKDIR /usr/src/app

RUN cp -rf /usr/share/fonts/ImgGen.exe.config .

ENTRYPOINT [ "mono" ]
CMD [ "ImgGen.exe", "./cards.cdb" ]
