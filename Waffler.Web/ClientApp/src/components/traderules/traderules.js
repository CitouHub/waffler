import React, { useState, useEffect } from "react";
import Button from '@mui/material/Button';
import Files from "react-files";

import LoadingBar from '../../components/utils/loadingbar';
import TradeRuleTable from './table/traderule.table';

import TradeRuleService from '../../services/traderule.service';

import './table.css';

const TradeRules = () => {
    const [loading, setLoading] = useState(true);
    const [tradeRules, setTradeRules] = useState([]);
    const [tradeRuleAttributes, setTradeRuleAttributes] = useState({});

    const updateTradeRules = () => {
        setLoading(true);
        var getTradeRules = TradeRuleService.getTradeRules();
        var getTradeRuleAttributes = TradeRuleService.getTradeRuleAttributes();

        Promise.all([getTradeRules, getTradeRuleAttributes]).then((result) => {
            setTradeRules(result[0]);
            setTradeRuleAttributes(result[1]);
            setLoading(false);
        });
    }

    const newTradeRule = () => {
        setLoading(true);
        TradeRuleService.newTradeRule().then((result) => {
            setTradeRules([...tradeRules, result]);
            setLoading(false);
        });
    }

    useEffect(() => {
        updateTradeRules();
    }, []);

    let fileReader = new FileReader();
    fileReader.onload = event => {
        setLoading(true);
        let content = JSON.parse(event.target.result);
        TradeRuleService.importTradeRule(content).then((result) => {
            if (result === true) {
                updateTradeRules();
            } else {
                setLoading(false);
            }
        });
    };

    return (
        <div>
            <LoadingBar active={loading} />
            <div className='mt-3 mb-3 control'>
                <h4>Rules</h4>
                <div>
                    <Button variant="text" disabled={loading} className='mr-3'>
                        <Files
                            className="import-trade-rule"
                            disabled={loading}
                            onChange={file => {
                                fileReader.readAsText(file[file.length - 1]);
                            }}
                            onError={err => console.log(err)}
                            accepts={[".json"]}
                            multiple={false}
                            maxFileSize={10000}
                            minFileSize={0}
                            clickable
                        >
                            + Import trade rule
                        </Files>
                    </Button>
                    <Button variant="contained" onClick={newTradeRule} disabled={loading}>+ Add new trade rule</Button>
                </div>
            </div>
            {tradeRules.length > 0 && Object.keys(tradeRuleAttributes).length > 0 &&
                <TradeRuleTable
                    tradeRules={tradeRules}
                    tradeRuleAttributes={tradeRuleAttributes}
                    updateTradeRules={updateTradeRules}
                />
            }
        </div>
    )
};

export default TradeRules;
