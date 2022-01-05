# Waffler
Waffler is a trade-aid application which acts on the Bitpanda platform via your own API-key. You can get your own API-key via your Bitpanda account here [Bitpanda API](https://exchange.bitpanda.com/account/keys). The idea is to set up rules for when to buy crypto assets based on price trends. To give Waffler its full functionality you need to provide an API key with both `Read` and `Trade` permissions.

## Running Waffler on your Raspberry Pi
Seting up Waffler on your own Raspberry Pi should be quite streight forward. Copy and run the docker-compose file `docker-compose.gh.arm64.yml` on your Raspberry Pi
```
docker-compose --file docker-compose.gh.arm64.yml up -d
```
In order to make the application work properly localy on your network, you'll also have to setup Avahi on your Raspberry Pi
```
apt-get update
apt-get install avahi-utils
```
Put the following in `/etc/systemd/system/avahi-alias@.service`
```
[Unit]
Description=Publish %I as alias for %H.local via mdns
Wants=network-online.target
After=network-online.target
Wants=docker.service
After=docker.service

[Service]
Type=simple
ExecStart=/bin/bash -c "/usr/bin/avahi-publish -a -R %I $(avahi-resolve -4 -n %H.local | cut -f 2)"

[Install]
WantedBy=multi-user.target
```
To make Waffler available as `waffler.local` on your local network (this will not interfere with `umbrel.local`) enable the following service:
```
sudo systemctl enable --now avahi-alias@waffler.local.service
```
To make sure that Waffler starts when you restart your Raspberry Pi put the following in `/etc/systemd/system/waffler.service`
```
[Unit]
Description=Docker Compose Waffler
Requires=docker.service
After=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
ExecStart=docker-compose --file /home/waffler/docker-compose.gh.arm64.yml up -d
ExecStop=docker-compose --file /home/waffler/docker-compose.gh.arm64.yml stop
TimeoutStartSec=0

[Install]
WantedBy=multi-user.target
```
And enable the Waffler service
```
sudo systemctl enable --now waffler.service
```
Waffler is now running on `http://waffler.local:8088`, enjoy!

## Trade rules
I'm experimeted a bit in designing some default trade rules. If you want to use these for your instance of Waffler you'll be able to find them in the 'Waffler.TradeRules' folder. After you've set up Waffler you can just import them in the `Trade rule` view.

## Final thoughts
I'm running Waffler on my own Raspberry Pi 4 which is also acting as my Lightning Node with Umbrel. The Sell-functionality is, in the spirit of HODL, by default disable, who wants to sell BTC? If, however, you want to enable it. Open your docker-compose file and change `Bitpanda__OrderFeature__Sell=false` to `Bitpanda__OrderFeature__Sell=true`. Oh! And the sell logic is not implemented yet so... yeah, don't sell your BTC :D
