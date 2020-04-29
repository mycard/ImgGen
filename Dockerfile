FROM debian:buster-slim as sqlite-builder

RUN apt-get update && \
	apt-get -y install wget sqlite3 libsqlite3-dev p7zip-full mono-complete && \
	rm -rf /var/lib/apt/lists/*
WORKDIR /
RUN wget https://minio.mycard.moe:9000/nanahira/sqlite-netFx-full-source-1.0.112.0.zip && \
	7z x -y -osqlite sqlite-netFx-full-source-1.0.112.0.zip && \
	cd sqlite && \
	xbuild /p:Configuration=Release /p:UseInteropDll=false /p:UseSqliteStandard=true ./System.Data.SQLite/System.Data.SQLite.2015.csproj

FROM debian:buster-slim as builder

RUN apt-get update && \
	apt-get -y install mono-complete && \
	rm -rf /var/lib/apt/lists/*

COPY . /ImgGen
WORKDIR /ImgGen
COPY --from=sqlite-builder /sqlite/bin/2015/ReleaseMonoOnPosix/bin/System.Data.SQLite.dll .
RUN xbuild /p:Configuration=Release /p:TargetFrameworkVersion=v4.6

FROM debian:buster-slim

RUN apt-get update && \
	apt-get -y install wget p7zip-full mono-complete xfonts-utils fontconfig && \
	rm -rf /var/lib/apt/lists/*

COPY --from=builder /ImgGen/bin/Release /usr/src/app
WORKDIR /usr/src/app
RUN wget https://minio.mycard.moe:9000/nanahira/ImgGen-Fonts.7z && \
	7z x -y  ImgGen-Fonts.7z && \
	rm -rf ImgGen-Fonts.7z && \
	cp -rf fonts /usr/share/ && \
	mkfontscale && \
	mkfontdir && \
	fc-cache -fv

ENTRYPOINT [ "mono" ]
CMD [ "ImgGen.exe", "./cards.cdb" ]
