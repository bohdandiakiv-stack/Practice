#!/bin/sh
set -x
apk update
apk add --no-cache curl

until curl -s http://couchbase:8091/ui/index.html; do
  echo "Waiting for Couchbase..."
  sleep 5
done

curl -s -u Administrator:password -X POST http://couchbase:8091/node/controller/setupServices \
  -d 'services=kv,n1ql'

curl -s -u Administrator:password -X POST http://couchbase:8091/nodes/self/controller/settings \
  -d path=/opt/couchbase/var/lib/couchbase/data

curl -s -X POST http://couchbase:8091/settings/web \
  -d port=8091 \
  -d username=Administrator \
  -d password=password

curl -s -u Administrator:password -X POST http://couchbase:8091/pools/default \
  -d memoryQuota=256

curl -s -u Administrator:password -X POST http://couchbase:8091/pools/default/buckets \
  -d name=TaskManager \
  -d ramQuotaMB=128 \
  -d bucketType=couchbase