const TTL = 1000 * 60 * 10; // 10 minutes

export default {
    set: (key, value) => {
		const now = new Date()

		const item = {
			value: value,
			expiry: now.getTime() + TTL,
		}
		localStorage.setItem(key, JSON.stringify(item))
	},
	get(key) {
		const itemStr = localStorage.getItem(key);

		if (!itemStr) {
			return null
		}
		const item = JSON.parse(itemStr)
		const now = new Date()

		if (now.getTime() > item.expiry) {
			localStorage.removeItem(key)
			return null
		}

		return item.value;
	},
	getAndReset(key) {
		const value = this.get(key);

		if (value) {
			this.set(key, value);
		}

		return value;
	}
}