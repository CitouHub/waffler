export default {
    getMinutes(timeUnitId, value) {
        switch (timeUnitId) {
            case 1: return value;
            case 2: return value * 60;
            case 3: return value * 60 * 24;
            case 4: return value * 60 * 24 * 7;
            default: return value;
        }
    },
    getUnit(timeUnitId) {
        switch (timeUnitId) {
            case 1: return 'Minutes';
            case 2: return 'Hours';
            case 3: return 'Days';
            case 4: return 'Weeks';
            default: return 'Unknown';
        }
    }
}