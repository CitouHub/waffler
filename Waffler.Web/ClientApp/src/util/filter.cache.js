export default {
	setFromDate: (fromDate) => {
		localStorage.removeItem('fromDate');
		localStorage.setItem('fromDate', JSON.stringify(fromDate));
	},
	setSelectedTradeRules: (selectedTradeRules) => {
		localStorage.removeItem('selectedTradeRules');
		localStorage.setItem('selectedTradeRules', JSON.stringify(selectedTradeRules));
	},
	setSelectedTradeOrderStatuses: (selectedTradeOrderStatuses) => {
		localStorage.removeItem('selectedTradeOrderStatuses');
		localStorage.setItem('selectedTradeOrderStatuses', JSON.stringify(selectedTradeOrderStatuses));
	},
	getFromDate: () => {
		const itemStr = localStorage.getItem('fromDate');
		if (!itemStr || itemStr === null) {
			return null;
		}
		return new Date(JSON.parse(itemStr));
	},
	getSelectedTradeRules: () => {
		const itemStr = localStorage.getItem('selectedTradeRules');
		if (!itemStr) {
			return [];
		}
		return JSON.parse(itemStr);
	},
	getSelectedTradeOrderStatuses: () => {
		const itemStr = localStorage.getItem('selectedTradeOrderStatuses');
		if (!itemStr) {
			return [];
		}
		return JSON.parse(itemStr);
	},
}