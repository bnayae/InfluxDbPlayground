Echo off
docker run -p 8089:8089 -p 8086:8086 -v influxdb.conf:/etc/influxdb/influxdb.conf:ro influxdb -config /etc/influxdb/influxdb.conf
pause