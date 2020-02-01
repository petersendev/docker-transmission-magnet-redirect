# docker-transmission-magnet-redirect
docker image for a web protocol handler for magnet links which adds the download to transmission

# Usage

```
docker run -it --name magnet \
  --restart unless-stopped \
  -p 9092:9092 \
  -e Transmission__Host=transmission.example.com \
  -e Transmission__Port=9091 \
  petersendev/transmission-magnet-redirect
```
