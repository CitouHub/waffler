# Waffler
Waffler is a trade-aid application which acts on the Bitpanda platform via your own API-key. You can get your own API-key via your Binpanda account here [Bitpanda API](https://exchange.bitpanda.com/account/keys). The idea is to set up rules for when to buy crypto assets based on price trends. To give Waffler its full functionality you need to provide an API key with both `Read` and `Trade`.

## Running Waffler on your Raspberry Pi
Seting up Waffler on your own Raspberry Pi should be quite streightforward. Copy and run the docker-compose file docker-compose.gh.arm64.yml on your Raspberry Pi
```
docker-compose --file docker-compose.gh.arm64.yml up
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

[Service]
Type=simple
ExecStart=/bin/bash -c "/usr/bin/avahi-publish -a -R %I $(avahi-resolve -4 -n %H.local | cut -f 2)"

[Install]
WantedBy=multi-user.target
```
To make Waffler avalible as `Waffler.local` enable the following service:
```
sudo systemctl enable --now avahi-alias@waffler.local.service
```
Waffler is now running on `http://waffler.local:8088`, enjoy!

## Trade rules
I'm experimeted a bit in designing some default trade rules. If you want to use these for your instance of Waffler you'll be able to get them i the 'Waffler.TradeRules' folder. After you've set up Waffler you can just import them in the `Trade rule` view.

## Thoughts
I'm running Waffler on my own Raspberry Pi 4 which is also acting as my Lightning Node with Umbrel.
