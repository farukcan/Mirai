FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /source
COPY /. ./

# Install npm - we need node_modules folder
RUN apk add --update nodejs npm
RUN npm install

# Build the app
RUN dotnet restore
RUN dotnet build -c Release
RUN dotnet publish -c Release -o /output

# Test it before run
RUN dotnet test --logger:trx

# we need fast and lightweight runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS runtime

# add globalization support
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# installs required packages
# RUN apk add libgdiplus --no-cache
RUN apk add --no-cache libc-dev

# Copy builds - we need fast app!
COPY --from=build /output .
COPY --from=build /source/node_modules ./node_modules

# Run the app - NOTE: Do not forget ENVs
EXPOSE 80/tcp
ENTRYPOINT ["./Mirai"]